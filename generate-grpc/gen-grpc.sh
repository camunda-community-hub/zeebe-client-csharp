#!/bin/bash

zeebeVersion='0.14.0'
protoFile=gateway.proto
gwProtoPath=./
genPath=Client/Impl/proto

# go to root
echo -e "cd ./\n"
cd ../

# restore packages
echo -e "nuget restore Zeebe.sln\n"
nuget restore Zeebe.sln

# get gatway proto file
echo -e "wget https://raw.githubusercontent.com/zeebe-io/zeebe/${zeebeVersion}/gateway-protocol/src/main/proto/gateway.proto\n"
wget https://raw.githubusercontent.com/zeebe-io/zeebe/${zeebeVersion}/gateway-protocol/src/main/proto/${protoFile}


# generate gRPC
echo "
 .packages/Grpc.Tools.1.16.0/tools/linux_x64/protoc \
  -I/usr/include/ \
  -I${gwProtoPath} \
  --csharp_out ${genPath} \
  --grpc_out ${genPath} \
  ${gwProtoPath}/${protoFile} \
  --plugin=\"protoc-gen-grpc=~/.nuget/packages/grpc.tools/1.15.0/tools/linux_x64/grpc_csharp_plugin\""
echo -e "\n"

./packages/Grpc.Tools.1.16.0/tools/linux_x64/protoc \
  -I/usr/include/ \
  -I${gwProtoPath} \
  --csharp_out ${genPath} \
  --grpc_out ${genPath} \
  ${gwProtoPath}/${protoFile} \
  --plugin="protoc-gen-grpc=packages/Grpc.Tools.1.16.0/tools/linux_x64/grpc_csharp_plugin"

# clean up
echo "rm ${protoFile}"
rm ${protoFile}
