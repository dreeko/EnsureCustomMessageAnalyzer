# EnsureCustomMessageAnalyzer Roslyn Analyzer project

## Purpose

When developing a user facing application utilizing the Fluent Validation library, its often a requirement to
implement domain and user specific error messages and formats, this analyzer will check existing code to ensure that the default messages
that are returned from FluentValidation builtin validators are overridden.
