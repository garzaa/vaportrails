for FILE in ./Assets/Achievements/Images/*.png; do
	FNAME="$(basename $FILE .png)"
	echo $FNAME

	# upscale to 256x256 with nearest neighbor scaling
	# overlay it on top of the steam frame
	# export as jpg in the steam out/ folder with Got
	magick convert "./Assets/Achievements/steamFrame.png" \
		"$FILE" \
		-gravity Center \
		-composite \
		-filter point \
		-resize 256x256 \
		-quality 100% \
		"./Steam/out/${FNAME}Got.JPG"

	# do it again with Gray
	magick convert "./Assets/Achievements/steamFrameGray.png" \
		"$FILE" \
		-gravity Center \
		-composite \
		-filter point \
		-colorspace Gray \
		-resize 256x256 \
		-quality 100% \
		"./Steam/out/${FNAME}Gray.JPG"
done

