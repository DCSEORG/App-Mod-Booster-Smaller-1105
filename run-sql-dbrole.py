#!/usr/bin/env python3
"""
run-sql-dbrole.py
Configures the managed-identity database user and role assignments.

Reads script.sql, substitutes the MANAGED-IDENTITY-NAME placeholder with the
value of the MANAGED_IDENTITY_NAME environment variable, then executes the
resulting SQL against Azure SQL using Azure AD (CLI) authentication.

Environment variables (set by sourcing AgentVariables.sh):
  SQL_SERVER_FQDN       – fully qualified domain name of the Azure SQL server
  SQL_DATABASE          – name of the target database
  ManagedIdentityName   – the user-assigned managed identity name to register

Usage:
  source AgentVariables.sh
  python3 run-sql-dbrole.py
"""

import os
import sys
import struct
import subprocess
import tempfile
import shutil
import pyodbc
from azure.identity import AzureCliCredential

# ── Configuration ──────────────────────────────────────────────────────────────
SERVER               = os.environ.get("SQL_SERVER_FQDN", "").strip()
DATABASE             = os.environ.get("SQL_DATABASE", "Northwind").strip()
MANAGED_IDENTITY     = os.environ.get("ManagedIdentityName", "mid-AppModAssist").strip()
SQL_SCRIPT_FILE      = "script.sql"

SQL_COPT_SS_ACCESS_TOKEN = 1256


def get_access_token() -> bytes:
    """Obtain an Azure SQL access token via Azure CLI credential."""
    credential = AzureCliCredential()
    token = credential.get_token("https://database.windows.net/.default")
    token_bytes = token.token.encode("utf-16-le")
    token_struct = struct.pack(f"<I{len(token_bytes)}s", len(token_bytes), token_bytes)
    return token_struct


def parse_sql_batches(sql_text: str) -> list[str]:
    """Split a SQL script on GO statement boundaries."""
    batches = []
    current: list[str] = []
    for line in sql_text.splitlines():
        stripped = line.strip()
        if stripped.upper() == "GO":
            batch = "\n".join(current).strip()
            if batch:
                batches.append(batch)
            current = []
        else:
            current.append(line)
    remainder = "\n".join(current).strip()
    if remainder:
        batches.append(remainder)
    return batches


def replace_placeholder_in_file(src_path: str, placeholder: str, replacement: str) -> str:
    """
    Copy src_path to a temp file, replace placeholder with replacement,
    and return the path to the temp file.
    Uses sed for cross-platform compatibility (macOS: sed -i .bak).
    """
    tmp_dir  = tempfile.mkdtemp()
    tmp_file = os.path.join(tmp_dir, os.path.basename(src_path))
    shutil.copy2(src_path, tmp_file)

    # sed -i.bak for macOS compatibility; the .bak file is removed afterwards
    subprocess.run(
        ["sed", "-i.bak", f"s/{placeholder}/{replacement}/g", tmp_file],
        check=True,
    )
    bak = tmp_file + ".bak"
    if os.path.exists(bak):
        os.remove(bak)

    return tmp_file


def main() -> int:
    if not SERVER:
        print("✗  SQL_SERVER_FQDN environment variable is not set.")
        print("   Source AgentVariables.sh before running this script.")
        return 1

    if not os.path.isfile(SQL_SCRIPT_FILE):
        print(f"✗  SQL script file not found: {SQL_SCRIPT_FILE}")
        return 1

    print(f"→  Server           : {SERVER}")
    print(f"→  Database         : {DATABASE}")
    print(f"→  Managed identity : {MANAGED_IDENTITY}")
    print(f"→  Script           : {SQL_SCRIPT_FILE}")

    # Replace placeholder in a temp copy of the script
    try:
        tmp_script = replace_placeholder_in_file(
            SQL_SCRIPT_FILE, "MANAGED-IDENTITY-NAME", MANAGED_IDENTITY
        )
        print(f"✓  Placeholder replaced → {tmp_script}")
    except Exception as exc:
        print(f"✗  Failed to substitute placeholder: {exc}")
        return 1

    # Read and parse
    with open(tmp_script, "r", encoding="utf-8") as fh:
        sql_text = fh.read()
    batches = parse_sql_batches(sql_text)
    print(f"→  Parsed {len(batches)} SQL batch(es)")

    # Obtain access token
    try:
        token_struct = get_access_token()
        print("✓  Obtained Azure AD access token")
    except Exception as exc:
        print(f"✗  Failed to obtain access token: {exc}")
        print("   Make sure you are logged in: az login")
        return 1

    conn_str = (
        "DRIVER={ODBC Driver 18 for SQL Server};"
        f"SERVER={SERVER};"
        f"DATABASE={DATABASE};"
        "Encrypt=yes;"
        "TrustServerCertificate=no;"
        "Connection Timeout=30;"
    )

    try:
        conn = pyodbc.connect(conn_str, attrs_before={SQL_COPT_SS_ACCESS_TOKEN: token_struct})
        conn.autocommit = True
        cursor = conn.cursor()
        print("✓  Connected to Azure SQL Database")
    except Exception as exc:
        print(f"✗  Connection failed: {exc}")
        return 1

    errors = 0
    for idx, batch in enumerate(batches, start=1):
        preview = batch.replace("\n", " ")[:80]
        try:
            cursor.execute(batch)
            print(f"✓  Batch {idx:>3}: {preview}")
        except Exception as exc:
            print(f"✗  Batch {idx:>3}: {preview}")
            print(f"           Error: {exc}")
            errors += 1

    cursor.close()
    conn.close()

    # Clean up temp file
    shutil.rmtree(os.path.dirname(tmp_script), ignore_errors=True)

    if errors:
        print(f"\n✗  Completed with {errors} error(s).")
        return 1

    print(f"\n✓  Database role configuration completed successfully.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
