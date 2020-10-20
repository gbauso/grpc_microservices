FROM python:3-slim-buster

ENV PORT=50052
# Create app directory
WORKDIR /app

# Install app dependencies
COPY src/requirements.txt ./

RUN pip install -r requirements.txt

# Bundle app source
COPY src /app

EXPOSE $PORT
CMD [ "python", "app.py" ]