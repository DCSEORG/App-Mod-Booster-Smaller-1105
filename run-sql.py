#!/usr/bin/env python3
"""
run-sql.py
Imports the database schema into Azure SQL using Azure AD (CLI) authentication.

Environment variables (set by sourcing AgentVariables.sh):
  SQL_SERVER_FQDN  – fully qualified domain name of the Azure SQL server
  SQL_DATABASE     – name of the target database

Usage:
  source AgentVariables.sh
  python3 run-sql.py
"""

import os
import sys
import struct
import pyodbc
from azure.identity import AzureCliCredential

# ── Configuration ──────────────────────────────────────────────────────────────
SERVER       = os.environ.get("SQL_SERVER_FQDN", "").strip()
DATABASE     = os.environ.get("SQL_DATABASE", "Northwind").strip()
SQL_SCRIPT_FILE = "Database-Schema/database_schema.sql"

SQL_COPT_SS_ACCESS_TOKEN = 1256  # pyodbc constant for passing an access token


def get_access_token() -> bytes:
    """Obtain an Azure SQL access token via Azure CLI credential."""
    credential = AzureCliCredential()
    token = credential.get_token("https://database.windows.net/.default")
    # pyodbc expects a specially-packed bytes value for SQL_COPT_SS_ACCESS_TOKEN
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
    # Capture any trailing statements without a trailing GO
    remainder = "\n".join(current).strip()
    if remainder:
        batches.append(remainder)
    return batches


def main() -> int:
    if not SERVER:
        print("✗  SQL_SERVER_FQDN environment variable is not set.")
        print("   Source AgentVariables.sh before running this script.")
        return 1

    if not os.path.isfile(SQL_SCRIPT_FILE):
        print(f"✗  SQL script file not found: {SQL_SCRIPT_FILE}")
        return 1

    print(f"→  Server   : {SERVER}")
    print(f"→  Database : {DATABASE}")
    print(f"→  Script   : {SQL_SCRIPT_FILE}")

    # Read and parse the SQL script
    with open(SQL_SCRIPT_FILE, "r", encoding="utf-8") as fh:
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

    # Build connection string (no password – token auth)
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

    # Execute each batch
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

    if errors:
        print(f"\n✗  Completed with {errors} error(s).")
        return 1

    print(f"\n✓  Schema imported successfully ({len(batches)} batch(es) executed).")
    return 0


if __name__ == "__main__":
    sys.exit(main())
