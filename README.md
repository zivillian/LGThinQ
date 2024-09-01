# LG ThinQ reverse engineering

## My setup

I have an LG washing machine `F4WV709P1E.ABWQWDG` with a ThinQ integration. The cloud integration was never used and I'm trying to figure out as much as I can and if possible make it work without letting the machine talk to the cloud.

## WIFI Setup

There is a hotspot named `LG_*_ABCD` the [last four digits are the wifi password](https://www.lg.com/us/support/help-library/lg-washer-troubleshooting-the-wifi-connection--20152621442788) in the form `ABCDABCD`.

Something listens on 192.168.120.254:5500

## mitmproxy

I've setup an additional VLAN with it's own SSID and a VM with dnsmasq & mitmproxy. Thus I can control the DNS responses and force all traffic through mitmproxy.

### initial connect

The first request was only send once - all other setup tries skipped the first request. All reset procedures I could find did not help. Looks like the device is tied to a region domain after the initial pairing - even if it failed. But it looks like the device is shipped with the domain `common.lgthinq.com` and updates its hostname via the `/route` endpoint. Future updates then need to happend at the now known domain.

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
1. when a matching certificate is returned the appliance falls back to the `/route` endpoint, but still uses the known domain
1. https://eic-common.lgthinq.com/route
    ```http
    GET https://eic-common.lgthinq.com/route
    x-service-code:   SVC202
    x-service-phase:  OP
    x-country-code:   DE
    ```
    if a 404 is returned the request is retried once.