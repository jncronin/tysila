FROM mcr.microsoft.com/dotnet/core/sdk:2.1 AS dotnet
WORKDIR /app

COPY build.sh /tmp
RUN chmod 777 /tmp/build.sh && /tmp/build.sh

ENV PATH="/usr/local/cross/bin:/usr/local/tymake:/usr/local/tysila:${PATH}"

COPY barebones /app
