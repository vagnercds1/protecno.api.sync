# protecno.api.sync
This project constitutes the API that comprises the Fixed Asset Inventory Platform. Throughout this project, I have implemented the latest concepts and technologies that I have acquired.

The API is developed using net6.0 and is intended to be deployed on AWS with App Runner.

The development process is ongoing.

The file labeled General Architecture.jpg depicts the complete AWS Architecture.

While all endpoints respond asynchronously, the fundamental operations at the core remain synchronous. This choice is made to ensure comprehensive data integrity when dealing with the numerous options within the database during usage.

All queries are paginated and utilize caching to ensure optimal performance and reduce the number of direct queries to the underlying database.

Any modifications undergo rigorous control with a fully dynamic history recording mechanism.

In the event of errors, the log service, powered by Serilog, writes all necessary information into MySQL. This information is crucial for subsequent analysis in Grafana.
