#!/bin/bash

protoPath=gateway-protocol/csharp-protocol
gwProtoPath=${protoPath}/target/proto
genPath=Client/Impl/proto

cd ${protoPath}

mvn clean install

cd ../../

echo "
~/.nuget/packages/grpc.tools/1.15.0/tools/linux_x64/protoc \
  -I/usr/include/ \
  -I${gwProtoPath} \
  --csharp_out ${genPath} \
  --grpc_out ${genPath} \
  ${gwProtoPath}/gateway.proto \
  --plugin=\"protoc-gen-grpc=~/.nuget/packages/grpc.tools/1.15.0/tools/linux_x64/grpc_csharp_plugin\""

./packages/Grpc.Tools.1.16.0/tools/linux_x64/protoc \
  -I/usr/include/ \
  -I${gwProtoPath} \
  --csharp_out ${genPath} \
  --grpc_out ${genPath} \
  ${gwProtoPath}/gateway.proto \
  --plugin="protoc-gen-grpc=packages/Grpc.Tools.1.16.0/tools/linux_x64/grpc_csharp_plugin"
