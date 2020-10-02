# simple-station
This is a simple station for a production line exposing values via an OPC UA server, which can be instantiated in three different types (Assembly, Test, Packaging) and is supposed to be controlled by a simple MES system, which is available as container [here](https://hub.docker.com/repository/docker/hansgschossmann/simple-mes).




# Usage

   
    Usage: simple-station.exe [<options>]
   
    Simple station for productionline simulation
    To exit the application, just press ENTER while it is running.
   
    Options:
          --lf, --logfile=VALUE  the filename of the logfile to use.
                                   Default: '<hostname>-station.log'
          --ll, --loglevel=VALUE the loglevel to use (allowed: fatal, error, warn,
                                   info, debug, verbose).
                                   Default: info
          --st, --stationtype=VALUE
                                 sets the station type
                                   Default: assemblystation
          --pn, --portnum=VALUE  the server port of the OPC server endpoint.
                                   Default: 51210
          --op, --path=VALUE     the enpoint URL path part of the OPC server
                                   endpoint.
                                   Default: ''
          --ga, --generatealerts the station should generate alerts.
                                   Default: False
          --pc, --powerconsumption=VALUE
                                 the stations average power consumption in kW
                                   Default:  150 kW
          --ct, --cycletime=VALUE
                                 the stations cycle time in seconds
                                   Default:  7000 sec
          --lr, --ldsreginterval=VALUE
                                 the LDS(-ME) registration interval in ms. If 0,
                                   then the registration is disabled.
                                   Default: 0
          --aa, --autoacceptcerts
                                 all certs are trusted when a connection is
                                   established.
                                   Default: False
          --to, --trustowncert   the application certificate is put into the
                                   trusted certificate store automatically.
                                   Default: False
          --at, --appcertstoretype=VALUE
                                 the own application cert store type.
                                   (allowed values: Directory, X509Store)
                                   Default: 'Directory'
          --ap, --appcertstorepath=VALUE
                                 the path where the own application cert should be
                                   stored
                                   Default (depends on store type):
                                   X509Store: 'CurrentUser\UA_MachineDefault'
                                   Directory: 'pki/own'
          --tp, --trustedcertstorepath=VALUE
                                 the path of the trusted cert store
                                   Default 'pki/trusted'
          --rp, --rejectedcertstorepath=VALUE
                                 the path of the rejected cert store
                                   Default 'pki/rejected'
          --ip, --issuercertstorepath=VALUE
                                 the path of the trusted issuer cert store
                                   Default 'pki/issuer'
          --csr                  show data to create a certificate signing request
                                   Default 'False'
          --ab, --applicationcertbase64=VALUE
                                 update/set this applications certificate with the
                                   certificate passed in as bas64 string
          --af, --applicationcertfile=VALUE
                                 update/set this applications certificate with the
                                   certificate file specified
          --pb, --privatekeybase64=VALUE
                                 initial provisioning of the application
                                   certificate (with a PEM or PFX fomat) requires a
                                   private key passed in as base64 string
          --pk, --privatekeyfile=VALUE
                                 initial provisioning of the application
                                   certificate (with a PEM or PFX fomat) requires a
                                   private key passed in as file
          --cp, --certpassword=VALUE
                                 the optional password for the PEM or PFX or the
                                   installed application certificate
          --tb, --addtrustedcertbase64=VALUE
                                 adds the certificate to the applications trusted
                                   cert store passed in as base64 string (multiple
                                   strings supported)
          --tf, --addtrustedcertfile=VALUE
                                 adds the certificate file(s) to the applications
                                   trusted cert store passed in as base64 string (
                                   multiple filenames supported)
          --ib, --addissuercertbase64=VALUE
                                 adds the specified issuer certificate to the
                                   applications trusted issuer cert store passed in
                                   as base64 string (multiple strings supported)
          --if, --addissuercertfile=VALUE
                                 adds the specified issuer certificate file(s) to
                                   the applications trusted issuer cert store (
                                   multiple filenames supported)
          --rb, --updatecrlbase64=VALUE
                                 update the CRL passed in as base64 string to the
                                   corresponding cert store (trusted or trusted
                                   issuer)
          --uc, --updatecrlfile=VALUE
                                 update the CRL passed in as file to the
                                   corresponding cert store (trusted or trusted
                                   issuer)
          --rc, --removecert=VALUE
                                 remove cert(s) with the given thumbprint(s) (
                                   multiple thumbprints supported)
      -h, --help                 show this message and exit
   
