# Calix GigaSpire LED Fade
I did this on the `Calix GigaSpire BLAST u6` while the internet was down and needed something to do. 

All this does is fade the LED on and off.

I only really worked on this for a day, so it's rough around the edges.

If you wanted to do this yourself or extend it, then the process is below.

### Authentication Process
1. Fetch a `nonce` from `http://192.168.1.1/get_nonce.cmd`
2. Get an MD5 hash with the format of `username:nonce:password` which works as our Auth
3. Fetch authentication cookie by sending a POST request to `http://192.168.1.1/login.cgi` with our username, auth, and nonce.
4. Store the cookie as a header for future calls.