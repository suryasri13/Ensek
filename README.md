# Ensek
Ensek api automation code

- You can run the tests by going into Ensek.Tests folder (in this folder, you will also find the .sln file) and run
  ```
  dotnet test
- Upon execution, you will see a failing test, this is a bug where user is not able to delete existing order using below endpoint
  ```
  DELETE /Ensek/orders/{orderId}
