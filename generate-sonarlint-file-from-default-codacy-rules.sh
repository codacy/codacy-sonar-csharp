#!/bin/env bash

container_id=$(docker create codacy-csharp:latest)
codacy_rules_file=/tmp/codacy-rules.json

# get all the rules defined for the tool from inside the docker container
docker cp $container_id:/docs/patterns.json $codacy_rules_file
docker rm $container_id

# obtain all ids of the rules we have enabled by default
rules_ids=$(cat $codacy_rules_file | jq -r '.patterns[] | select (.enabled == true) | .patternId')

cat << EOF > SonarLint.xml
<?xml version="1.0" encoding="UTF-8"?>
<AnalysisInput>
<Rules>
EOF

for rule_id in $rules_ids
do
	echo -e "\t<Rule>\n\t\t<Key>$rule_id</Key>\n\t</Rule>\n" >> SonarLint.xml
done

cat << EOF >> SonarLint.xml
</Rules>
</AnalysisInput>
EOF
