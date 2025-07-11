# Wolfgang.Etl.Abstractions

This package contains interfaces and base classes for building ETLs using a specific design pattern

The ETL design pattern is a common approach in data processing that involves three main stages:
- **Extract**: Retrieving data from various sources.
- **Transform**: Processing and transforming the extracted data into a desired format.
- **Load**: Storing the transformed data into a target system.

The abstractions in this package provide a way to define and implement these stages 
in a flexible and reusable manner. Each stage can be implemented with or without 
support for cancellation and progress reporting, allowing for greater control 
over the ETL process.

To build an ETL using this package, you would typically need to create 5 classes:
- One class for each of the three stages: Extract, Transform, and Load.
- One class representing the source data.
- One class representing the target data.
- One class that acts as the ETL orchestrator, coordinating the execution of the three stages.

The design uses lazy loading and lazy evaluation to ensure that data is processed only when needed.
This allows for efficient memory usage and can handle large datasets without loading everything into memory at once.

The process uses a pull method rather than a push method to move data through the pipeline.
The process starts when the ETL orchestrator calls the `LoadAsyc` method of the `Loader` class. 
The loader will start enumerating through the list of items passed into its `LoadAsync` method.
This will intern trigger the `TransformAsync` method of the `Transformer` class, which will process each item 
and yield the transformed results. The process of transformation will also trigger the `ExtractAsync` method of the `Extractor` class,
which will retrieve the necessary data from the source.

For more information check out the documentation