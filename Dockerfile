# Etapa 1: Compilación (Build con .NET 10)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar el archivo de proyecto y restaurar dependencias
COPY ["EcommerceApp.csproj", "./"]
RUN dotnet restore "EcommerceApp.csproj"

# Copiar el resto del código fuente
COPY . .
RUN dotnet publish "EcommerceApp.csproj" -c Release -o /app/publish

# Etapa 2: Ejecución (Runtime con .NET 10)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Instalar dependencias del sistema necesarias para Npgsql (Kerberos/GSSAPI)
RUN apt-get update && apt-get install -y libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Render asigna un puerto dinámico mediante la variable PORT
ENV ASPNETCORE_URLS=http://+:$PORT

# Ejecutar la aplicación
ENTRYPOINT ["dotnet", "EcommerceApp.dll"]