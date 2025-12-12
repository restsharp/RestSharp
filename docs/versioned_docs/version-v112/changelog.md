---
title: What's new
description: List of changes for the current major version
sidebar_position: 1
---

# Changelog

For release notes of previous versions, please check the [Releases page](https://github.com/restsharp/RestSharp/releases) in RestSharp GitHub repository.

Changes between major versions are documented in the documentation for each version on this website.

# v112.0

* Security fix for [CVE-2024-45302](https://github.com/restsharp/RestSharp/security/advisories/GHSA-4rr6-2v9v-wcpc). Header values cannot contain `CRLF`.

## v112.1

* Follow up on v112.0 security fix: remove `\t` from the list of forbidden characters in headers.