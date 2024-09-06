# LG ThinQ reverse engineering

## My setup

I have an LG washing machine `F4WV709P1E.ABWQWDG` with a ThinQ integration. The cloud integration was never used and I'm trying to figure out as much as I can and if possible make it work without letting the machine talk to the cloud.

A few days later I've found https://github.com/anszom/rethink ...

## WIFI Setup

There is a hotspot named `LG_*_ABCD` the [last four digits are the wifi password](https://www.lg.com/us/support/help-library/lg-washer-troubleshooting-the-wifi-connection--20152621442788) in the form `ABCDABCD`.

Something listens on 192.168.120.254:5500

## mitmproxy

I've setup an additional VLAN with it's own SSID and a VM with dnsmasq & mitmproxy. Thus I can control the DNS responses and force all traffic from the device through mitmproxy.

### initial connect

The first request was only send once - all other setup tries skipped the first request. All reset procedures I could find did not help. Looks like the device is tied to a region domain after the initial pairing - even if it failed. But it looks like the device is shipped with the domain `common.lgthinq.com` and updates its hostname via the `/route` endpoint. Future updates then need to happen at the now known domain.

1. https://common.lgthinq.com/route
    ```http
    GET https://common.lgthinq.com/route
    User-Agent:       IOE Client
    x-service-code:   SVC202
    x-service-phase:  OP
    x-country-code:   DE
    
    {
        "result": {
            "apiServer": "https://eic-common.lgthinq.com:443",
            "mqttServer": "ssl://a3phael99lf879.iot.eu-west-1.amazonaws.com:8883"
        },
        "resultCode": "0000"
    }
    ```
1. It looks like there are other request, which fail due to certificate pinning
1. https://eic-common.lgthinq.com/route/certificate
    ```http
    GET https://eic-common.lgthinq.com/route/certificate
    User-Agent:     IOE Client

    {
        "result": [
            "common-server",
            "aws-iot"
        ],
        "resultCode": "0000"
    }

    ```
1. https://eic-common.lgthinq.com/route/certificate?name=common-server
    ```http
    GET https://eic-common.lgthinq.com/route/certificate?name=common-server
    User-Agent:     IOE Client

    {
        "result": {
            "certificatePem": "-----BEGIN CERTIFICATE-----\n
            MIIEMjCCAxqgAwIBAgIBATANBgkqhkiG9w0BAQUFADB7MQswCQYDVQQGEwJHQjEb\n
            MBkGA1UECAwSR3JlYXRlciBNYW5jaGVzdGVyMRAwDgYDVQQHDAdTYWxmb3JkMRow\n
            GAYDVQQKDBFDb21vZG8gQ0EgTGltaXRlZDEhMB8GA1UEAwwYQUFBIENlcnRpZmlj\n
            YXRlIFNlcnZpY2VzMB4XDTA0MDEwMTAwMDAwMFoXDTI4MTIzMTIzNTk1OVowezEL\n
            MAkGA1UEBhMCR0IxGzAZBgNVBAgMEkdyZWF0ZXIgTWFuY2hlc3RlcjEQMA4GA1UE\n
            BwwHU2FsZm9yZDEaMBgGA1UECgwRQ29tb2RvIENBIExpbWl0ZWQxITAfBgNVBAMM\n
            GEFBQSBDZXJ0aWZpY2F0ZSBTZXJ2aWNlczCCASIwDQYJKoZIhvcNAQEBBQADggEP\n
            ADCCAQoCggEBAL5AnfRu4ep2hxxNRUSOvkbIgwadwSr+GB+O5AL686tdUIoWMQua\n
            BtDFcCLNSS1UY8y2bmhGC1Pqy0wkwLxyTurxFa70VJoSCsN6sjNg4tqJVfMiWPPe\n
            3M/vg4aijJRPn2jymJBGhCfHdr/jzDUsi14HZGWCwEiwqJH5YZ92IFCokcdmtet4\n
            YgNW8IoaE+oxox6gmf049vYnMlhvB/VruPsUK6+3qszWY19zjNoFmag4qMsXeDZR\n
            rOme9Hg6jc8P2ULimAyrL58OAd7vn5lJ8S3frHRNG5i1R8XlKdH5kBjHYpy+g8cm\n
            ez6KJcfA3Z3mNWgQIJ2P2N7Sw4ScDV7oL8kCAwEAAaOBwDCBvTAdBgNVHQ4EFgQU\n
            oBEKIz6W8Qfs4q8p74Klf9AwpLQwDgYDVR0PAQH/BAQDAgEGMA8GA1UdEwEB/wQF\n
            MAMBAf8wewYDVR0fBHQwcjA4oDagNIYyaHR0cDovL2NybC5jb21vZG9jYS5jb20v\n
            QUFBQ2VydGlmaWNhdGVTZXJ2aWNlcy5jcmwwNqA0oDKGMGh0dHA6Ly9jcmwuY29t\n
            b2RvLm5ldC9BQUFDZXJ0aWZpY2F0ZVNlcnZpY2VzLmNybDANBgkqhkiG9w0BAQUF\n
            AAOCAQEACFb8AvCb6P+k+tZ7xkSAzk/ExfYAWMymtrwUSWgEdujm7l3sAg9g1o1Q\n
            GE8mTgHj5rCl7r+8dFRBv/38ErjHT1r0iWAFf2C3BUrz9vHCv8S5dIa2LX1rzNLz\n
            Rt0vxuBqw8M0Ayx9lt1awg6nCpnBBYurDC/zXDrPbDdVCYfeU0BsWO/8tqtlbgT2\n
            G9w84FoVxp7Z8VlIMCFlA2zs6SFz7JsDoeA3raAVGI/6ugLOpyypEBMs1OUIJqsi\n
            l2D4kF501KKaU73yqWjgom7C12yxow+ev+to51byrvLjKzg6CYG1a4XXvi3tPxq3\n
            smPi9WIsgtRqAEFQ8TmDn5XpNpaYbg==\n
            -----END CERTIFICATE-----\n"
        },
        "resultCode": "0000"
    }
    ```
1. when a matching certificate is returned the appliance falls back to the `/route` endpoint, but still uses the known domain. The country code is taken from the account of the app.
1. https://eic-common.lgthinq.com/route
    ```http
    GET https://eic-common.lgthinq.com/route
    x-service-code:   SVC202
    x-service-phase:  OP
    x-country-code:   US

    {
        "resultCode": "0000",
        "result": {
            "apiServer": "https://aic-common.lgthinq.com:443",
            "mqttServer": "ssl://a3phael99lf879.iot.us-west-2.amazonaws.com:8883"
        }
    }
    ```
    If the response format does not match (i.e. a typo in `mqqtServer`) the request is retried.
1. If the reponse sets the `common-server` endpoint to `https://common.lgthinq.com:443` the next request is:
    ```http
    POST https://common.lgthinq.com/device/<GUID>/certificate
    User-Agent:      IOE Client
    Content-Type:    application/json
    {
        "ciphertext": "<base64>",
        "csr": "-----BEGIN CERTIFICATE REQUEST-----
                <base64>
                -----END CERTIFICATE REQUEST-----\n",
        "otp": "<base64>",
        "publickey": "-----BEGIN PUBLIC KEY-----
                      <base64>
                      -----END PUBLIC KEY-----"
    }

    <unknown>
    ```
    1. if a `404` is returned the request is retried two times with a 5 second delay
    1. the `ciphertext`, `csr` and `publickey` is different in each request - `otp` stays the same (for these three requests)
    1. if relaying to lgthinq
        1. and the body misses any property the error is `Invalid Body`
        1. if `csr`, `otp` and `publickey` are set to `"-"` the ciphertext is validated. If it is modified in any way the error is `Invalid ciphertext: Encryption/decryption failed.`
        1. this means the ciphertext is independent of the other parameters
    1. `otp` is base64 encoded type-4 UUID
    1. `GUID` in the URL is a type-1 UUID with an invalid timestamp (3215-05-22)
    1. the `GUID` is stable, but the `otp` changes for each pairing request
        1. is the app creating a pairing request in lgthinq and sending the OTP to the device?
    1. if the request is send to the lgthink servers with `csr` and `publickey` replaced by `-` the response is
        ```json
        {
            "result": {},
            "resultCode": "0002",
            "reason": "random number not found for 25 sec"
        }
        ```
    1. if the request (with `csr` and `publickey` replaced by `-`) is send fast enough the response is
        ```json
        {
            "result": {},
            "resultCode": "0002",
            "reason": "csrHash: <256bit hex> != 3973e022e93220f9212c18d0d0c543ae7c309e46640da93a4a0314de999f5112"
        }
        ```
        `3973e022e93220f9212c18d0d0c543ae7c309e46640da93a4a0314de999f5112` is the result of `echo -n - | sha256sum`
        
        `256bit hex` is the result of `sha256(<expected base64 csr with linebreaks>)`
        1. either ciphertext contains the csrHash or lgthinq already knows the csrHash and correlates by device GUID / otp / ciphertext
    1. if the same request (with `csr` and `publickey` replaced by `-`) is send a minute later the response is again `random number not found for 25 sec`
    1. if it is sent an hour later the response is
        ```json
        {
            "result": {},
            "resultCode": "0002",
            "reason": "OTP not found, random number also not found"
        }
        ```
## 192.168.120.254:5500

- the app talks to this endpoint using TLS 1.2.
- the device has a self signed certificate for `Internet Widgits Pty Ltd`
- when connecting with `openssl s_client -connect 192.168.120.254:5500` the device sends `{"cmd": "ping", "type":"request"}`
- when replying with `enter` or `{"cmd": "ping", "type":"response"}` the connection is closed

## frida

With [frida](https://github.com/frida/frida) I was able to capture the communication to 192.168.120.254:5500 using:

`frida-trace -U -i SSL_read -i SSL_write -N com.lgeha.nuts`

The scripts are in the frida folder.

The request for the ping response seems to be `{"type":"request","cmd":"pong","data":{"constantConnect":"Y"}}`

The communication between the appliance and the cloud looks like this:

1. request getDeviceInfo
    ```json
    {
        "type": "request",
        "cmd": "getDeviceInfo",
        "data": {
            "subCountryCode": "DE",
            "regionalCode": "eic",
            "timezone": "+0100",
            "publicKey": "-----BEGIN PUBLIC KEY-----\n<base64>\n-----END PUBLIC KEY-----\n",
            "instantFailReason": "Y",
            "constantConnect": "Y"
        }
    }
    ```

1. response getDeviceInfo
    ```json
    {
        "type": "response",
        "cmd": "getDeviceInfo",
        "data": {
            "protocolVer": "3.1",
            "demandType": "RTK_RTL8711am",
            "mac": "64:cb:e9:XX:XX:XX",
            "uuid": "<GUID>",
            "encrypt_val": "<base64>",
            "deviceType": "201",
            "modelName": "F_V8_Y___W.B_2QEUK",
            "softwareVer": "2.10.123",
            "eepromChecksum": "0",
            "countryCode": "WW",
            "modemVer": "clip_hna_v1.9.110",
            "errorcodeDisplay": "0",
            "remainingTime": "420",
            "errorCode": "nS",
            "errSSID": "intercept-right",   "//":"yep, that's the name of my wifi",
            "subErrorCode": "NONE",
            "extra": "CLP_RESET_REASON_WATCHDOG|16124163546601407478|0",
            "factoryResetCode": "56500",
            "supportsWpa3": "Y",
            "agentVer": "0"
        }
    }
    ```

1. request getDeviceInfo
    seems to be resent - publicKey is identical
    ```json
    {
        "type": "request",
        "cmd": "getDeviceInfo",
        "data": {
            "subCountryCode": "DE",
            "regionalCode": "eic",
            "timezone": "+0100",
            "publicKey": "-----BEGIN PUBLIC KEY-----\n<base64>\n-----END PUBLIC KEY-----\n",
            "instantFailReason": "Y",
            "constantConnect": "Y"
        }
    }
    ```

1. response getDeviceInfo
    ```json
    {
        "type": "response",
        "cmd": "getDeviceInfo",
        "data": {
            "protocolVer": "3.1",
            "demandType": "RTK_RTL8711am",
            "mac": "64:cb:e9:XX:XX:XX",
            "uuid": "<GUID>",
            "encrypt_val": "<base64>",
            "deviceType": "201",
            "modelName": "F_V8_Y___W.B_2QEUK",
            "softwareVer": "2.10.123",
            "eepromChecksum": "0",
            "countryCode": "WW",
            "modemVer": "clip_hna_v1.9.110",
            "errorcodeDisplay": "0",
            "remainingTime": "420",
            "errorCode": "nS",
            "errSSID": "intercept-right",
            "subErrorCode": "NONE",
            "extra": "CLP_RESET_REASON_WATCHDOG|16124163546601407478|0",
            "factoryResetCode": "57713",
            "supportsWpa3": "Y",
            "agentVer": "0"
        }
    }
    ```

1. request setCertInfo
    ```json
    {
        "type": "request",
        "cmd": "setCertInfo",
        "data": {
            "otp": "<base64 encoded uuid>",
            "svccode": "SVC202",
            "svcphase": "OP",
            "constantConnect": "Y"
        }
    }
    ```

1. response setCertInfo
    ```json
    {
        "type": "response",
        "cmd": "setCertInfo",
        "data": {
            "protocolVer": "3.1",
            "subErrorCode": "NONE",
            "result": "000"
        }
    }
    ```

1. request setApInfo
    ```json
    {
        "type": "request",
        "cmd": "setApInfo",
        "data": {
            "format": "B64",
            "ssid": "aW50ZXJjZXB0LWxlZnQ=", "//":"base64 encoded SSID - intercept-left",
            "security": "WPA2_PSK",
            "cipher": "AES",
            "password": "aW50ZXJjZXB0",     "//":"base64 endoded password - intercept",
            "constantConnect": "Y"
        }
    }
    ```

1. some ping requests and pong responses
    ```json
    {
        "cmd": "ping",
        "type": "request"
    }
    {
        "type": "request",
        "cmd": "pong",
        "data": {
            "constantConnect": "Y"
        }
    }
    ```

1. response setApInfo
    ```json
    {
        "type": "response",
        "cmd": "setApInfo",
        "data": {
            "result": "999",
            "rssi": "-65",
            "protocolVer": "3.1",
            "subErrorCode": "W017",
            "errorCode": "nD"
        }
    }
    ```

1. ping / pong
1. response setApInfo
    ```json
    {
        "type": "response",
        "cmd": "setApInfo",
        "data": {
            "result": "999",
            "rssi": "-65",
            "protocolVer": "3.1",
            "subErrorCode": "W021",
            "errorCode": "nS"
        }
    }
    ```

1. ping / pong until fail

## Findings

### clip.com

the subject fo the csr is [CN=*.clip.com, O=LGE, C=KR](https://clip.com/)

### the server seems to use python 

the `/device/<GUID>/certificate` endpoint returns the error [Invalid ciphertext: Ciphertext length must be equal to key size.](https://github.com/pyca/cryptography/blob/e343723356e29f22d74516e251c87ed829c59667/src/rust/src/backend/rsa.rs#L322) if the ciphertext parameter does not match the csr or publickey.

### LG is using AWS IOT Core for ThinQ

not a surprise but [confirmed by Amazon](https://aws.amazon.com/de/solutions/case-studies/lg-electronics/). Good to know it was not build on AWS but migrated to AWS.