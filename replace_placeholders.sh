#!/bin/bash

# Function to replace placeholders in a file
replace_placeholders() {
    local file=$1
    local placeholder=$2
    local value=$3
    sed -i "s|#{$placeholder}#|$value|g" "$file"
}

# Path to your appsettings.json
settings_file="/app/appsettings.json"

# Replace JWT_SECRET
if [ ! -z "$JWT_SECRET" ]; then
    replace_placeholders "$settings_file" "JWT_SECRET" "$JWT_SECRET"
fi

# Replace SMTP_PASSWORD
if [ ! -z "$SMTP_PASSWORD" ]; then
    replace_placeholders "$settings_file" "SMTP_PASSWORD" "$SMTP_PASSWORD"
fi

# Start the application
dotnet API.dll