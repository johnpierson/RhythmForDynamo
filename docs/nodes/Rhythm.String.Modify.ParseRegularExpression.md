## In Depth

`Modify.ParseRegularExpression(stringToReplace, regexString: "[^a-zA-Z0-9]", replacement: "")`

This will run a regular expression on a a string. By default this removes all whitespace and special characters from a string

The inputs are:

- `stringToReplace` (_string_) — Your target string.
- `regexString` (_string_, defaults to `"[^a-zA-Z0-9]"`) — The regular expression to use.
- `replacement` (_string_, defaults to `""`) — What to replace with.

Returns `modifiedString` (_string_) — The finished product

Found in the library under **Actions**.
