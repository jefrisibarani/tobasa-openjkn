## Create https certificate
```dotnet dev-certs https -ep Tobasa.Wepapp\wsopenjkn_cert.pfx -p Mkerinci3805```


## Run app, from solution dir
```
dotnet build
dotnet run --project Tobasa.WebApp --urls http://0.0.0.0:8084
```

## Run published app
Run from _publish folder inside solution dir

