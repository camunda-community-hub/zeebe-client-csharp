#!/bin/bash

os=windows_x64
grpcVersion=1.17.0
zeebeVersion='0.16.1'
protoFile=gateway.proto
gwProtoPath=./
genPath=Client/Impl/proto

# go to root
echo -e "cd ../\n"
cd ../

# restore packages
echo -e "nuget restore Zeebe.sln\n"
nuget restore Zeebe.sln

# get gatway proto file
echo -e "curl -X GET https://raw.githubusercontent.com/zeebe-io/zeebe/${zeebeVersion}/gateway-protocol/src/main/proto/gateway.proto > ${protoFile}\n"
curl -X GET https://raw.githubusercontent.com/zeebe-io/zeebe/${zeebeVersion}/gateway-protocol/src/main/proto/${protoFile} > ${protoFile}


# generate gRPC
echo "
 .packages/Grpc.Tools.${grpcVersion}/tools/${os}/protoc.exe \
  -I/usr/include/ \
  -I${gwProtoPath} \
  --csharp_out ${genPath} \
  --grpc_out ${genPath} \
  ${gwProtoPath}/${protoFile} \
  --plugin=\"protoc-gen-grpc=./packages/Grpc.Tools.${grpcVersion}/tools/${os}/grpc_csharp_plugin.exe\""
echo -e "\n"

./packages/Grpc.Tools.${grpcVersion}/tools/${os}/protoc.exe \
  -I${gwProtoPath} \
  --csharp_out ${genPath} \
  --grpc_out ${genPath} \
  ${gwProtoPath}/${protoFile} \
  --plugin="protoc-gen-grpc=./packages/Grpc.Tools.${grpcVersion}/tools/${os}/grpc_csharp_plugin.exe"

# clean up
echo "rm ${protoFile}"
rm ${protoFile}
