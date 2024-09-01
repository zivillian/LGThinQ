# LG Thinq reverse engineering

## WIFI Setup

There is a hotspot named `LG_*_ABCD` the [last four digits are the wifi password](https://www.lg.com/us/support/help-library/lg-washer-troubleshooting-the-wifi-connection--20152621442788) in the form `ABCDABCD`.

Something listens on 192.168.120.254:5500

## mitmproxy

### initial connect

The first request was only send once - all other setup tries skipped the first request. All reset procedures I could find did not help. Looks like the device is tied to a region after the initial pairing - even if it failed.
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