#!/bin/bash

VAULT_KEYS="$VAULT_UNSEAL_KEY_1 $VAULT_UNSEAL_KEY_2 $VAULT_UNSEAL_KEY_3 $VAULT_UNSEAL_KEY_4 $VAULT_UNSEAL_KEY_5 $VAULT_UNSEAL_KEY_6 $VAULT_UNSEAL_KEY_7 $VAULT_UNSEAL_KEY_8 $VAULT_UNSEAL_KEY_9 $VAULT_UNSEAL_KEY_10 $VAULT_UNSEAL_KEY_11 $VAULT_UNSEAL_KEY_12 $VAULT_UNSEAL_KEY_13 $VAULT_UNSEAL_KEY_14 $VAULT_UNSEAL_KEY_15"

i=0
for k in $VAULT_KEYS; do
    i=$((i+1))
    vault status;
    st=$?

    if [ $st -eq 0 ]; then
        echo "vault is unsealed"
        exit 0
    elif [ $st -eq 2 ]; then
        echo "vault is sealed"
        echo "unsealing with key $i"

        if [ -z "$k" ]; then
            echo "ran out of vault uneal keys at $i (VAULT_UNSEAL_KEY_$i is missing). terminating..."
            exit 1
        fi

        vault operator unseal "$k" > /dev/null
        code=$?
        if [ $? -ne 0 ] ; then
            echo "unseal returned a bad exit code ($code). terminating..."
            exit $code
        fi

    elif [ $st -eq 1 ]; then
        echo "vault returned an error"
        exit 1
    fi
done
