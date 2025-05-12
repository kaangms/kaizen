#!/bin/bash

# Length (default: 128 characters)
LENGTH=${1:-128}

# Karakter kümesi
CHARSET='A-Za-z0-9!@#$%^&*()-_=+[]{}|;:,.<>?'

# Generate random key
KEY=$(cat /dev/urandom | tr -dc "$CHARSET" | head -c $LENGTH)

echo "Generated Key:"
echo "$KEY"
echo "Length: $LENGTH"