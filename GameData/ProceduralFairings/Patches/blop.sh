#!/bin/bash

while read line; do
	cat <<DONE
                VARIANT
                {
                        name = ${line}
                        displayName = ${line}
                        primaryColor = #ff0000
                        secondaryColor = #0000ff

                        EXTRA_INFO
                        {
                                textureSet = ${line}
                        }
                }
DONE
done
