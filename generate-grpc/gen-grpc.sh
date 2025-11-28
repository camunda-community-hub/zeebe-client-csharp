#!/bin/bash
set -euxo pipefail

os=linux_x64
grpcVersion=2.64.0
packagePath=~/.nuget/packages/grpc.tools/${grpcVersion}/tools/${os}/
zeebeVersion='8.8.0'
protoFile=gateway.proto
gwProtoPath=./
genPath=Client/Impl/proto

url="https://raw.githubusercontent.com/camunda/camunda/${zeebeVersion}/zeebe/gateway-protocol/src/main/proto/${protoFile}"


DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
cd $DIR

# go to root
echo -e "cd ../\n"
cd ../

# restore packages
echo -e "nuget restore Zeebe.sln\n"
dotnet restore Zeebe.sln

# get gatway proto file
echo -e "curl -X GET $url > ${protoFile}\n"
curl -X GET "$url" > ${protoFile}


# generate gRPC
echo "${packagePath}/protoc \
  -I/usr/include/ \
  -I${gwProtoPath} \
  --csharp_out ${genPath} \
  --grpc_out ${genPath} \
  ${gwProtoPath}/${protoFile} \
  --plugin=\"protoc-gen-grpc=${packagePath}/grpc_csharp_plugin\""
echo -e "\n"

${packagePath}/protoc \
  -I/usr/include/ \
  -I${gwProtoPath} \
  --csharp_out ${genPath} \
  --grpc_out ${genPath} \
  ${gwProtoPath}/${protoFile} \
  --plugin="protoc-gen-grpc=${packagePath}/grpc_csharp_plugin"

# clean up
echo "rm ${protoFile}"
rm ${protoFile}
