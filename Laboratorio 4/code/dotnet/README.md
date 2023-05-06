# Lab Functions

## Create/Update function

Creates or updates a note with DynamoDB put item command. Required the UserId, NoteId, and note text. Returns the note Id.

- Test event

```json
{
  "UserId": "student",
  "NoteId": "999",
  "Note": "test note"
}
```

- Output

```text
"999"
```

## Delete function

Deletes a note with a DynamoDB delete item command. Requires the UserId and NoteId. Returns the NoteId that was deleted.

- Test event

```json
{
  "UserId": "student",
  "NoteId": "999"
}
```

- Output

```text
"999"
```

## Dictate function

Converts a note to speech with Amazon Polly. This function has one environment variable that is the Amazon S3 bucket name that polly saves the MP3 file in. A signed URL is also generated for that object and returned by the function. Requires a UserId, NoteId, and polly voiceId to be input.  The function returns a signed URL for the MP3 file that was placed in S3 by Polly.  

- Test event

```json
{
  "UserId": "student",
  "NoteId": "999",
  "voiceId": "Joey"
}
```

- Output

```text
"https://labstack-bailess-ku1lfuh8wqjz-pollynotesapibucket-1kcygopiz4xgb.s3.amazonaws.com/student/999.mp3?AWSAccessKeyId=ASIAW4TGVI4PHY3LX3P5&Signature=uCnmPOQYQ%2B2OTDR5%2BIfxM0zs%2BZc%3D&x-amz-security-token=IQoJb3JpZ2luX2VjEEsaCXVzLXdlc3QtMiJHMEUCIFFPNRCCvE65z2C50gpktbYXlfQ7SuRZnA9WUL0R0OjVAiEApoRTiqDMDpOmVC22ThQCekKxglfquSSxw7a2VpNavJEqnQII9P%2F%2F%2F%2F%2F%2F%2F%2F%2F%2FARABGgw0NzM3MzU0NDgzNTAiDHM%2Bg6ccVHQdPtQKHSrxAZ37Xx64kEf4TvYxI7mvRCOveF2uz7sMLp324izD8HksVbQs0%2BZkA30pT1sf%2B5XQQgXxE7hpz1V1491kHQ6xQMnivuoupSNB3vtQ%2BBqbIF8ffyYiBlCHrxlNMas%2FQFSdYCWMo4sUWwQShC0gg%2BgHHSIB3aN6YwebY2EzdGyzdWsZkH04hrl2%2FBLmeLkXsDGtN8binYybqyHYI%2BwW83q6E5lBjIkCaTT7GivJo9US%2Bj7forO0qNMzqQen3g8ncSSowjnhBzGe7nt51c8snqtUjzs73ZOSDZ9kH1JcHKXz1%2FTik7N6%2BzLVWhF9SXCo9BG%2FnRQwwrzkhQY6mgHz5jIEQ3H4IiS%2BVoHLLuo3vQXhvnnyJnWIt22sJMXAPKd%2FLtVZzdUD%2Fg1Mt%2F6tbat3BpEmnT3qUnr8TAvx65a402J71eohtIvI1emHmgZqou6FbdyjcUHn7rszwwm16pfu7l8fzO5VKrFS77XXOd3I%2BJMc5OWfmxyc%2B0GZvzXOHVsoBFyGXFRPsCJ8w8u6qL5hWBxbEqZfxueI&Expires=1622748244"
```

## List function

This function performs a DynamoDB query command. It requires the UserId as an input. The function returns a JSON list of notes.

- Test event

```json
{
  "UserId": "student"
}
```

- Output

```json
[
  {
    "UserId": "student",
    "Note": "My note to myself",
    "NoteId": "001"
  },
  {
    "UserId": "student",
    "Note": "A new note to myself",
    "NoteId": "002"
  }
]
```

## Search function

This function performs a DynamoDB query command with a filter expression. The function requires the UserId and text to query as an input. The function returns a JSON list of notes.

- Test event

```json
{
  "UserId": "student",
  "text": "test"
}
```

- Output

```json
[
  {
    "UserId": "student",
    "Note": "test note",
    "NoteId": "999"
  }
]
```
