#!/bin/bash

rawurlencode() {
  local string="${1}"
  local strlen=${#string}
  local encoded=""
  local pos c o

  for (( pos=0 ; pos<strlen ; pos++ )); do
     c=${string:$pos:1}
     case "$c" in
        [-_.~a-zA-Z0-9] ) o="${c}" ;;
        * )               printf -v o '%%%02x' "'$c"
     esac
     encoded+="${o}"
  done
  echo "${encoded}"
}

EMAIL=[your email]
API_KEY=[your key]
URL="https://epc.opendatacommunities.org/api/v1/domestic/search"

POSTCODE="OX1 2EP"
PAGE_SIZE=100
ADDRESS="43, Hythe Bridge Street"

QUERY="$URL?"

if [ ! -z "$PAGE_SIZE" ]; then
  QUERY="${QUERY}size=$PAGE_SIZE&"
fi

if [ ! -z "$ADDRESS" ]; then
  var=$(rawurlencode "$ADDRESS")
  QUERY="${QUERY}address=$var"
elif [ ! -z "$POSTCODE" ]; then
  var=$(rawurlencode "$POSTCODE")
  QUERY="${QUERY}postcode=$var&"
fi

TOKEN="$(echo -n $EMAIL:$API_KEY | base64 -w0)"

if [ -z "$ADDRESS" ]; then
  curl -s -H "Accept: application/json" -H "Authorization: Basic $TOKEN" $QUERY | jq | grep '"address":'
else
  curl -s -H "Accept: application/json" -H "Authorization: Basic $TOKEN" $QUERY | jq
fi
