# emulatorhttpstream

Emulator for HTTP Stream - Web App

Run EmulatorWebApp Project

Use Postman to send data (in any json format you need) to HTTP Stream republish the messages. If you need to send more one Json, you can use like na array.

Please use refer Postman Collection EmulatorHTTPStreamWeb.postman_collection.json

If you want to take a look on the stream, using a powershell, command:

curl.exe -N -u root:impinj http://localhost:5052/stream

-------------------------------------------------------

Emulator for HTTP Stream - Console App (Not working)

Make sure to grant access in your environment for URL's
Run CMD as Administrator

netsh http add urlacl url=http://localhost:5099/ user=Todos

netsh http add urlacl url=http://localhost:5099/sendData/ user=Todos

Run the Console Application

Use Postman to send data to HTTP Stream publish the messages, for this refer to Postman Collection EmuladorHTTPStream.postman_collection.json