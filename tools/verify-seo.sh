#!/usr/bin/env bash
# Fails the build when a page route is missing from the SWA config or sitemap,
# or when its prerendered HTML lacks an <h1>, <title> or canonical link.
# Usage: tools/verify-seo.sh <publish/wwwroot>
set -uo pipefail

out="${1:-publish/wwwroot}"
config="wwwroot/staticwebapp.config.json"
sitemap="wwwroot/sitemap.xml"
site="https://www.itsalldevtools.com"
errors=0
# Routes that redirect elsewhere (e.g. /pro while Pro is disabled): only checked for config + prerender.
redirect_routes=" pro "

fail() { echo "::error::$1"; errors=$((errors + 1)); }

routes=$(grep -ho '^@page "/[^"{]*"' Pages/*.razor | sed -E 's/^@page "\/(.*)"$/\1/' | sort -u)

while IFS= read -r r; do
  if [ -z "$r" ]; then
	page="$out/index.html"
	url="$site/"
  else
	page="$out/$r/index.html"
	url="$site/$r"
	grep -q "\"/$r/index.html\"" "$config" || fail "/$r has no rewrite to /$r/index.html in $config"
  fi

  is_redirect=0
  case "$redirect_routes" in *" $r "*) is_redirect=1 ;; esac

  [ "$is_redirect" = 1 ] || grep -q "<loc>$url</loc>" "$sitemap" || fail "$url is missing from $sitemap"

  if [ ! -f "$page" ]; then
	fail "/$r was not prerendered ($page missing)"
	continue
  fi
  grep -q '<h1' "$page" || fail "/$r prerendered HTML has no <h1>"
  grep -q '<title>' "$page" || fail "/$r prerendered HTML has no <title>"
  [ "$is_redirect" = 1 ] || grep -q "<link rel=\"canonical\" href=\"$url\"" "$page" || fail "/$r prerendered HTML has no canonical link to $url"
done <<< "$routes"

count=$(echo "$routes" | wc -l)
if [ "$errors" -gt 0 ]; then
  echo "SEO verification failed with $errors error(s) across $count routes."
  exit 1
fi
echo "SEO verification passed for $count routes."
