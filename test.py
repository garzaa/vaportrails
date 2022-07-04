from distutils.dir_util import copy_tree
from turtle import width


texWidth = 16
texHeight = 16
texture = [i for i in range(texWidth * texHeight)]
for i in range(len(texture)):
	texture[i] = str(texture[i]).zfill(3)

# remember it starts on the bottom left
def printTexture():
	row = 0
	for y in range(texHeight-1, -1, -1):
		line = str(y).zfill(2) + " [ "
		for x in range(texWidth):
			idx = (y * texWidth) + x
			line += str(texture[idx]).zfill(3) + " "
		line += "]"
		print(line)

	line = "      "
	for r in range(texWidth):
		line += str(r).zfill(2) + "  "
	print(line)

def ctoi(x,y):
	return (y * texWidth) + x

tileSize = 8
blocksPerTile = 1
blockSize = (int) (tileSize / blocksPerTile)

workingTile = {
	"x": 1,
	"y": 1
}

targetBlock = {
	"x": 0,
	"y": 0
}

originX = (workingTile["x"] * tileSize) + (targetBlock["x"] * blockSize)
originY = (workingTile["y"] * tileSize) + (targetBlock["y"] * blockSize)
print(str(originX) + ", " + str(originY))
texCoordStart = ctoi(originX, originY)
print(texCoordStart)
temp = texture[texCoordStart]
texture[texCoordStart] = "___"

printTexture()
texture[texCoordStart] = temp

print ("===== REVERSING TEXTURE =====")
# now...we get a list of target indices to reverse
# get all indices within that block
toReverse = []
for y in range(originY, originY+blockSize):
	for x in range(originX, originX+blockSize):
		idx = ctoi(x, y)
		# texture[idx] = "___"
		toReverse.append(idx)

xAxis = (int) (originX + blockSize/2)
print(str(xAxis) + "\n")

other = None
for i in toReverse:
	# pivot it around the central point
	# 200 <-> 203
	# 201 <-> 202
	# DIFF NEEDS TO GO DOWN BY 2 FUCK
	diff = (blockSize - (i % blockSize)*2 - 1)
	if (i%blockSize < blockSize/2):
		print(str(i) + ": " + str((i % blockSize)*2) + ": "+ str(diff))
		other = texture[i + diff]
		texture[i + diff] = texture[i]
		texture[i] = other

printTexture()
