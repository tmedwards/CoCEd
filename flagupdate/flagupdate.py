#!/usr/bin/python3

import requests

results = requests.get("https://raw.githubusercontent.com/Ormael7/Corruption-of-Champions/master-wip/classes/classes/GlobalFlags/kFLAGS.as")
results2 = results.text.split('\n')

fcount = 0

with open ("checkflag.txt", "w+") as f:
	for i in results2:
		if "public static const" in i:
			i = i.replace("public static const ","	<Flag ID=\"" + str(fcount) + "\"	Name=\"")
			if "//" in i:	#Comments
				j = i.split("//",1)[1]
				j2 = j.strip()
				#print (j2)
				if not j2:
					i = i.replace(":int","\"/>?").split("?",1)[0]
					f.write(i)
				elif "Description=" in j2:
					print (j)
					#print (i)
					j3 = j.find("Description")
					j3 = (j[j3:]).strip() + "\"/>"
					i = i.replace(":int", "\"")
					k = i.split("=",3)[3]	#Cleans away =   1; similars
					i = i.replace(k, "")[:-1] + j3
					f.write(i)
				else:
					j2 = "Description=\"" + j2 + "\"/>"
					i = i.replace(":int", "\"").replace(j, "")
					k = i.split("=",3)[3]
					i = i.replace(k, "")[:-1]
					f.write(i + j2)
			else:		#No comments
				i = i.replace(":int","\"/>?").split("?",1)[0]
				f.write(i)
			f.write("\n")
			fcount += 1
