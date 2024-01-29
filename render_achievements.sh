# for everything in the achievements/images folder
for FILE in ./Assets/Achievements/Images/*.png; do
	# magick convert $FILE 
	FNAME="$(basename $FILE .png)"
	echo $FNAME
	# image magick upscale to 256x256 with nearest neighbor scaling
	# add a purple background, get the hex code
	# export as jpg in the steam out/ folder with Got
	magick convert "$FILE" -filter point -background "#211158" -alpha remove -alpha off -resize 256x256 -quality 100% "./Steam/out/${FNAME}Got.JPG"
	# then open it, desaturate it, and do it again with Gray
	magick convert "./Steam/out/${FNAME}Got.JPG" -colorspace Gray "./Steam/out/${FNAME}Gray.JPG"
done

