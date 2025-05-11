#!/bin/bash

# Length (default: 64 characters)
LENGTH=${1:-64}

# Karakter kümesi
CHARSET='A-Za-z0-9!@#$%^&*()-_=+[]{}|;:,.<>?'

# Generate random key
KEY=$(cat /dev/urandom | tr -dc "$CHARSET" | head -c $LENGTH)

echo "Generated Key:"
echo "$KEY"
echo "Length: $LENGTH"