<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<!--
	Index for Battleship online game
	
	This screen is where users log in to and it will create a cookie verifying their log-in
  -->
<?php session_start(); ?>

<html xmlns="http://www.w3.org/1999/xhtml">

<head>
	<title>Online Battlship</title>
	<meta name="description" content="Online battleship game for a school project. Not for profit." />
	<meta name="keywords" content="video games, battleship" />
	
	<link rel="stylesheet" type="text/css" href="php/media/style.css" />
	<!-- <link rel="icon" type="image/png" href="media/gs_videogamelogo.png" / -->
	<!-- <link rel="shortcut icon" href="media/gs_videogamelogo.ico" /> -->
</head>

<body>
<div id="outer">
<div id="wrapper">

	<!-- Header with Title -->
	<div id="header">
		<img src="php/media/thickbar.png" />
		<br />
		<br />
		<?php include("php/header.php"); ?>
		<br />
	</div>
	
	<!--
	<div id="navbar">
		<ul>
		<?php include("php/navbar.php"); ?>
		</ul>
		<img class="thinbar" src="media/thinbar.png" />
	</div>
	-->

	
	<div id="bodywrapper">

		<!-- login bar -->		
		<div id="bodyright">
			<?php include("php/login.php"); ?>
		</div>

		<div id="bodyleft">
		<!-- Content -->
		<br />
		<img class="thinbarbody" src="php/media/thinbar.png" />
		<h2>Welcome</h2>
		
		<div>
		<p>
Battleship Online is a FAST PACED, HEART-RACING, LIFE-SAVING video game that will probably change your life forever. All you need to do is either Sign Up or Login if you are an existing member! Then the ADRENOLINE PUMPING BATTLESHIP ACTION can take place! 
<br /><br />
Join now! Or else you'll be sad.		
		</p>
		<img class="thinbarbody" src="php/media/thinbar_body.png" />
		</div>
	</div>
	
	<div id="footer">
		<?php include("php/footer.php"); ?>
		<img src="php/media/thickbar.png" />
	</div>
	
</div>
</div>
</body>

</html>
