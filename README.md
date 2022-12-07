The utility itself can be built using docker-compose:

    > docker-compose build pagefetcher
	
And then executed like so:

    > docker run --rm  -it pagefetcher
	> ./PageFetcher --metadata https://www.google.com https://www.example.com
	
There is also an --output-dir option you can specify (e.g. ./PageFetcher --output-dir /tmp/ --metadata https://www.google.com) in case you want to mount a volume to write it out to your desktop machine.

I started adding some tests, these can be built like so:

    > docker-compose build pagefetcher.tests
	
And then run:

    > docker run --rm -it pagefetcher.tests
	
Note that the tests are incomplete because I ran out of time.