FROM python:3.6-slim-buster

ENV PORT=50052
# Create app directory
WORKDIR /app

RUN echo 'deb http://deb.debian.org/debian buster main' >> /etc/apt/sources.list \
    && apt-get update && apt-get install -y gcc python3-dev

# Install app dependencies
COPY requirements.txt ./

RUN pip install -r requirements.txt

# Bundle app source
COPY src /app

EXPOSE $PORT
CMD python app.py