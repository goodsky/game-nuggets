<?php //login.php


if (isset($_POST['loginsubmit']))
{
	// store the username and password
	$username = $_POST['username'];
	$password = $_POST['password'];

	$con = mysql_connect("localhost", "ghost", "%fnIAsQl");

	if (!$con) echo "<p>Could not connect to database</p>";

	// mysql_select_db("goodskyvg");
	mysql_select_db("onlinebattleship");

	$epassword = md5($password);

	// $sql = "SELECT * FROM users WHERE username = '$username' AND password = '$epassword'";
	$sql = "SELECT * FROM users";

	// query the database to see if this username and password combination exists
	$result = mysql_query($sql);

	while ($row = mysql_fetch_array($result)) {
		echo("<p>ping</p>");
	}

	// see if this user exists
/*
	if ($row = mysql_fetch_array($result))
	{
	   //log in
	   $_SESSION['test'] = 13;
	   $_SESSION['usr'] = $username;
	   $_SESSION['psw'] = $password;

	   //$row = mysql_fetch_array($result);
	   $_SESSION['gamesplayed'] = $row['games'];
	   $_SESSION['gameswon'] = $row['games_won'];
	}
	else
	{
	   //no such user
	   echo "<p>Username/Password were not found. :(</p>";
	}
*/
	mysql_free_result($result);
	mysql_close($con);
}


// See if we need a log in screen or the 'Hello user!'
$test = $_SESSION['test'];

if ($test == 13)
{
	$gamesplayed = $_SESSION['gamesplayed'];
	$gameswon = $_SESSION['gameswon'];
	echo "<div class='centeredtext'>Hello <b>".$_SESSION['usr']."</b>!</div>";
	echo "<p>User Score:<br />";
	echo "<b>Total Games Won:</b> $gameswon<br />";
	echo "<b>Total Games Played:</b> $gamesplayed</p>";
	echo "<p>Good luck with all your games!</p>";
	echo "<p><div class='speciallink'><a href='/bo/logout.php'>Log Out</a></div></p>";
}
else 
{
	echo "<form method='post' action=''>";
	echo "<div>Member Login:</div>";
	echo "<div><label for='username'>username</label>: <input type='text' name='username' value='' id='username' /></div>";
	echo "<div><label for='password'>password</label>: <input type='password' name='password' value='' id='password' /></div>";
	echo "<div><input type='submit' name='loginsubmit' value='submit' /></div>";
	echo "</form>";

	echo "<p><div class='speciallink'><a href='/vg/register.php'>Become a Member</a></div></p>";
}

?>
