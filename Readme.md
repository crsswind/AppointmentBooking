# Enpal Coding Challenge

## Getting Started

Follow these steps to set up and run the project.

### 1. Set Up the Database

Navigate to the `Database` folder and execute the following commands:

```sh
docker build -t enpal-coding-challenge-db .
docker run --name enpal-coding-challenge-db -p 5432:5432 -d enpal-coding-challenge-db
```

### 2. Build the Application

Navigate to the root of the solution (`AppointmentBookingApi`) and run:

```sh
dotnet build
```

### 3. Run End-to-End Tests

Navigate to the `test-app` folder and install dependencies:

```sh
npm install
```

Then, run the tests:

```sh
npm run test
```

### 4. Run Unit Tests

Navigate to `AppointmentBookingApi` and execute the following command to run unit tests:

```sh
dotnet test
```

## Notes

- The application is configured to listen on **HTTPS on port 3000**. End-to-end tests have been updated accordingly to reflect this setup.
