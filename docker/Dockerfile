FROM microsoft/dotnet:1.0.0-preview2-sdk
MAINTAINER Roman Atachiants "roman@misakai.com"

# Make sure we have S3 & additional libraries
RUN apt-get update -qq \
	&& apt-get install -y git \
	&& mkdir /tmp/emitter \
	&& cd /tmp/emitter \
	&& git clone "https://github.com/kelindar/analytics-dashboard.git" "/tmp/emitter" \
	&& cd /tmp/emitter \
	&& dotnet restore -v Minimal \
	&& cd /tmp/emitter/src/Server \
	&& dotnet publish -c Release --output /app \
	&& rm -rf /tmp/emitter

# Application will be in app folder
WORKDIR /app
ADD deploy.sh /
CMD ["/bin/bash", "/deploy.sh"]