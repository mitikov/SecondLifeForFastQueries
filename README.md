# Second Life for FastQueries

[Sitecore Fast Query mechanism](https://doc.sitecore.com/SdnArchive/upload/sdn5/developer/using%20sitecore%20fast%20query/using%20sitecore%20fast%20query.pdf) allows fetching items via direct SQL queries.

This the only functionality allowing to query full Sitecore content.

Alternative approaches suffer from drawbacks:

* `Content Search` index should cover only certain parts of content tree ([5.8 Developer's Guide to Item Buckets and Search](https://doc.sitecore.com/legacy-docs/SC71/developers-guide-to-item-buckets-and-search-sc7-a4.pdf)), hence logic is to be developed to union results from different indexes. Secondly, being a metadata built from actual data, it may not be 100 % accurate all the time.
* `Sitecore query` is executed via application layer and gets all the performance penalties for data fetch/processing by buisness logic.

FastQuery is consuming limited SQL resource turning it into a bottleneck in almost every solution as volume of data grows.

Should the drawback be mitigated, Sitecore Fast Queries would become first class citizens again!

## Implementation detail

Publishing targets (f.e. `web` database) is known to be updated only by publishing ending with event (like `publish:end`).

It leaves space for caching Fast Query results to avoid hammering SQL server (since requests would produce exactly same results in case no data modifications in between).

`SelectSingleItem` and `SelectItems` method results are cached by a decorator put on top of stock Sitecore database impl.