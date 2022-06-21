# Project Purpose

This project is for the data contracts that we publish so that others can consume this service.

Internal domain specific objects that we don't want publish are located in the App project.

## Guidelines:

- This project must not have any other dependecies, including any other nuget packages
- All request and response types must be a custom object. This is done so that if needed, these objects can be extended later. Cases include
    - Types that have a single property
    - Lists - Don't return a list directly, put it inside another type.