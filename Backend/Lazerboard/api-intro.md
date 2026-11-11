Welcome to Lazerboard API documentation. You can use this API to get 
information about osu!lazer leaderboard statistics without pestering peppy.

# Rate Limiting
The API implements rate limiting to prevent abuse and ensure fair usage.
- **Global limit**: 60 requests per minute per IP address.
- **Queue limit**: up to 3 requests can be queued during bursts.
- **Rejection**: when the limit is exceeded, the API returns the 429 result with a `Retry-After` header indicating when to retry.

Please do your best to respect these limits in order to avoid temporary blocks.