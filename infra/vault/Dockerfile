FROM vault:1.5.0

RUN ls
COPY ./unseal.sh unseal.sh
RUN chmod a+x /unseal.sh

RUN ["server", "vault operator init" ,"/bin/sh", "/unseal.sh"]

