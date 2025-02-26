# emulatorhttpstream

Emulator for HTTP Stream

Make sure to grant access in your environment for URL's
Run CMD as Administrator
netsh http add urlacl url=http://localhost:5099/ user=Todos
netsh http add urlacl url=http://localhost:5099/sendData/ user=Todos

Run the Console Application

Use Postman to send data to HTTP Stream publish the messages, for this refer to Postman Collection EmuladorHTTPStream.postman_collection.json