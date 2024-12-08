∞4
\/home/lambo-ubuntu/Authentication/Authentifications/Services/AuthentificationBasicService.cs
	namespace 	
Authentifications
 
. 
Repositories (
;( )
public		 
class		 (
AuthentificationBasicService		 )
:		* +!
AuthenticationHandler		, A
<		A B'
AuthenticationSchemeOptions		B ]
>		] ^
{

 
private 
readonly	 -
!JwtBearerAuthenticationRepository 3-
!jwtBearerAuthenticationRepository4 U
;U V
private 
readonly	 *
JwtBearerAuthenticationService 0*
jwtBearerAuthenticationService1 O
;O P
public (
AuthentificationBasicService $
($ %-
!JwtBearerAuthenticationRepository% F-
!jwtBearerAuthenticationRepositoryG h
,h i+
JwtBearerAuthenticationService	j à,
jwtBearerAuthenticationService
â ß
,
ß ®
IOptionsMonitor
© ∏
<
∏ π)
AuthenticationSchemeOptions
π ‘
>
‘ ’
options
÷ ›
,
› ﬁ
ILoggerFactory 
logger 
, 

UrlEncoder 
encoder 
, 
ISystemClock 
clock 
) 
: 
base 
( 
options 
, 
logger 
, 
encoder  
,  !
clock" '
)' (
{ 
this 
. -
!jwtBearerAuthenticationRepository (
=) *-
!jwtBearerAuthenticationRepository+ L
;L M
this 
. *
jwtBearerAuthenticationService %
=& '*
jwtBearerAuthenticationService( F
;F G
} 
internal 	
async
 
Task 
< 
bool 
> 
AuthenticateAsync ,
(, -
string- 3
email4 9
,9 :
string; A
passwordB J
)J K
{ 
var 
utilisateur 
= -
!jwtBearerAuthenticationRepository 5
.5 6
GetUserByFilter6 E
(E F
emailF K
)K L
;L M
if 
( 
utilisateur 
== 
null 
) 
{ 
return 	
false
 
; 
} 
await 
Task 
. 
Delay 
( 
$num 
) 
; 
return 
utilisateur	 
. 
CheckHashPassword &
(& '
password' /
)/ 0
;0 1
} 
	protected   

override   
async   
Task   
<   
AuthenticateResult   1
>  1 2#
HandleAuthenticateAsync  3 J
(  J K
)  K L
{!! 
if"" 
("" 
!"" 
Request"" 
."" 
Headers"" 
."" 
ContainsKey"" "
(""" #
$str""# 2
)""2 3
)""3 4
return## 	
AuthenticateResult##
 
.## 
Fail## !
(##! "
$str##" @
)##@ A
;##A B
try$$ 
{%% 
var&& 

authHeader&& 
=&& %
AuthenticationHeaderValue&& -
.&&- .
Parse&&. 3
(&&3 4
Request&&4 ;
.&&; <
Headers&&< C
[&&C D
$str&&D S
]&&S T
)&&T U
;&&U V
if'' 
('' 
string'' 
.'' 
IsNullOrEmpty'' 
('' 

authHeader'' &
.''& '
	Parameter''' 0
)''0 1
||''2 4
!''5 6

authHeader''6 @
.''@ A
Scheme''A G
.''G H
Equals''H N
(''N O
$str''O V
,''V W
StringComparison''X h
.''h i
OrdinalIgnoreCase''i z
)''z {
)''{ |
return(( 

AuthenticateResult(( 
.(( 
Fail(( "
(((" #
$str((# H
)((H I
;((I J
var)) 
credentialBytes)) 
=)) 
Convert))  
.))  !
FromBase64String))! 1
())1 2

authHeader))2 <
.))< =
	Parameter))= F
!))F G
)))G H
;))H I
var** 
credentials** 
=** 
Encoding** 
.** 
UTF8** "
.**" #
	GetString**# ,
(**, -
credentialBytes**- <
)**< =
.**= >
Split**> C
(**C D
$char**D G
,**G H
$num**I J
)**J K
;**K L
var++ 
email++ 
=++ 
credentials++ 
[++ 
$num++ 
]++ 
;++ 
var,, 
password,, 
=,, 
credentials,, 
[,, 
$num,, 
],,  
;,,  !
if.. 
(.. 
credentials.. 
... 
Length.. 
!=.. 
$num.. 
).. 
return// 

AuthenticateResult// 
.// 
Fail// "
(//" #
$str//# H
)//H I
;//I J
if11 
(11 
await11 
AuthenticateAsync11 
(11 
email11 $
,11$ %
password11& .
)11. /
)11/ 0
{22 *
jwtBearerAuthenticationService33 "
.33" #
GenerateJwtToken33# 3
(333 4
email334 9
)339 :
;33: ;
}44 
return55 	
AuthenticateResult55
 
.55 
Fail55 !
(55! "
$str55" @
)55@ A
;55A B
}66 
catch77 
(77 	
FormatException77	 
)77 
{88 
return99 	
AuthenticateResult99
 
.99 
Fail99 !
(99! "
$str99" I
)99I J
;99J K
}:: 
catch;; 
(;; 	
	Exception;;	 
ex;; 
);; 
{<< 
return== 	
AuthenticateResult==
 
.== 
Fail== !
(==! "
$"==" $
$str==$ ;
{==; <
ex==< >
.==> ?
Message==? F
}==F G
"==G H
)==H I
;==I J
}>> 
}?? 
}@@ ÒG
^/home/lambo-ubuntu/Authentication/Authentifications/Services/JwtBearerAuthenticationService.cs
	namespace		 	
Authentifications		
 
.		 
Services		 $
;		$ %
public

 
class

 *
JwtBearerAuthenticationService

 +
:

, -
	IJwtToken

. 7
{ 
private 
readonly	 -
!JwtBearerAuthenticationRepository 3-
!jwtBearerAuthenticationRepository4 U
;U V
private 
readonly	 
IConfiguration  
configuration! .
;. /
public *
JwtBearerAuthenticationService &
(& '
IConfiguration' 5
configuration6 C
,C D-
!JwtBearerAuthenticationRepositoryE f.
!jwtBearerAuthenticationRepository	g à
)
à â
{ 
this 
. -
!jwtBearerAuthenticationRepository (
=) *-
!jwtBearerAuthenticationRepository+ L
;L M
this 
. 
configuration 
= 
configuration $
;$ %
} 
public 
bool 
CheckUserSecret 
( 
string #

secretPass$ .
). /
{ 
string 
secretUserPass	 
= 
configuration '
[' (
$str( H
]H I
;I J
if 
( 
string 
. 
IsNullOrEmpty 
( 
secretUserPass )
)) *
)* +
{ 
throw 
new	 
ArgumentException 
( 
$str ?
)? @
;@ A
} 
var 
Pass 

= 
BCrypt 
. 
Net 
. 
BCrypt 
. 
HashPassword +
(+ ,

secretPass, 6
)6 7
;7 8
var 
BCryptResult 
= 
BCrypt 
. 
Net 
.  
BCrypt  &
.& '
Verify' -
(- .
secretUserPass. <
,< =
Pass> B
)B C
;C D
if 
( 
! 
BCryptResult 
. 
Equals 
( 
true 
)  
)  !
{" #
return$ *
false+ 0
;0 1
}2 3
return 
true	 
; 
} 
public 
async 
Task 
< 
TokenResult 
> 
GetToken  (
(( )
string) /
email0 5
)5 6
{   
var!! 
utilisateur!! 
=!! -
!jwtBearerAuthenticationRepository!! 5
.!!5 6
GetUserByFilter!!6 E
(!!E F
email!!F K
,!!K L
	adminOnly!!M V
:!!V W
true!!X \
)!!\ ]
;!!] ^
await"" 
Task"" 
."" 
Delay"" 
("" 
$num"" 
)"" 
;"" 
return## 
new##	 
TokenResult## 
{$$ 
Response%% 
=%% 
true%% 
,%% 
Token&& 
=&&	 

GenerateJwtToken&& 
(&& 
utilisateur&& '
.&&' (
Email&&( -
!&&- .
)&&. /
}'' 
;'' 
}(( 
public)) 
string)) 
GetSigningKey)) 
()) 
))) 
{** 
RSA++ 
rsa++ 	
=++
 
RSA++ 
.++ 
Create++ 
(++ 
$num++ 
)++ 
;++ 
RSAParameters,, 

privateKey,, 
=,, 
rsa,,  
.,,  !
ExportParameters,,! 1
(,,1 2
true,,2 6
),,6 7
;,,7 8
RSAParameters-- 
	publicKey-- 
=-- 
rsa-- 
.--  
ExportParameters--  0
(--0 1
false--1 6
)--6 7
;--7 8
var// 
rsaSecurityKey// 
=// 
new// 
RsaSecurityKey// )
(//) *
rsa//* -
)//- .
;//. /
return00 
rsaSecurityKey00	 
.00 
ToString00  
(00  !
)00! "
;00" #
}11 
public22 
string22 
GenerateJwtToken22 
(22  
string22  &
email22' ,
)22, -
{33 
var44 
utilisateur44 
=44 -
!jwtBearerAuthenticationRepository44 5
.445 6
GetUserByFilter446 E
(44E F
email44F K
,44K L
	adminOnly44M V
:44V W
true44X \
)44\ ]
;44] ^
var55 
securityKey55 
=55 
new55  
SymmetricSecurityKey55 ,
(55, -
Encoding55- 5
.555 6
UTF8556 :
.55: ;
GetBytes55; C
(55C D
GetSigningKey55D Q
(55Q R
)55R S
)55S T
)55T U
;55U V
var66 
tokenHandler66 
=66 
new66 #
JwtSecurityTokenHandler66 0
(660 1
)661 2
;662 3
var77 
tokenDescriptor77 
=77 
new77 #
SecurityTokenDescriptor77 3
{88 
Subject99 

=99 
new99 
ClaimsIdentity99 
(99  
new99  #
[99# $
]99$ %
{99& '
new:: 
Claim::	 
(:: 

ClaimTypes:: 
.:: 
Name:: 
,:: 
utilisateur::  +
.::+ ,
Nom::, /
!::/ 0
)::0 1
,::1 2
new;; 
Claim;;	 
(;; 

ClaimTypes;; 
.;; 
Email;; 
,;;  
utilisateur;;! ,
.;;, -
Email;;- 2
!;;2 3
);;3 4
,;;4 5
new<< 
Claim<<	 
(<< 

ClaimTypes<< 
.<< 
Role<< 
,<< 
utilisateur<<  +
.<<+ ,
Role<<, 0
.<<0 1
ToString<<1 9
(<<9 :
)<<: ;
)<<; <
,<<< =
new== 
Claim==	 
(== #
JwtRegisteredClaimNames== &
.==& '
Jti==' *
,==* +
Guid==, 0
.==0 1
NewGuid==1 8
(==8 9
)==9 :
.==: ;
ToString==; C
(==C D
)==D E
)==E F
,==F G
new>> 
Claim>>	 
(>> #
JwtRegisteredClaimNames>> &
.>>& '
Iat>>' *
,>>* +
DateTimeOffset>>, :
.>>: ;
UtcNow>>; A
.>>A B
ToUnixTimeSeconds>>B S
(>>S T
)>>T U
.>>U V
ToString>>V ^
(>>^ _
)>>_ `
,>>` a
ClaimValueTypes>>b q
.>>q r
	Integer64>>r {
)>>{ |
}?? 
)@@ 
,@@ 
ExpiresAA 

=AA 
DateTimeAA 
.AA 
UtcNowAA 
.AA 
AddHoursAA %
(AA% &
$numAA& '
)AA' (
,AA( )
SigningCredentialsBB 
=BB 
newBB 
SigningCredentialsBB .
(BB. /
securityKeyBB/ :
,BB: ;
SecurityAlgorithmsBB< N
.BBN O
HmacSha512SignatureBBO b
)BBb c
,BBc d
AudienceCC 
=CC 
nullCC 
,CC 
IssuerDD 	
=DD
 
configurationDD 
.DD 

GetSectionDD $
(DD$ %
$strDD% 2
)DD2 3
[DD3 4
$strDD4 <
]DD< =
,DD= >
}EE 
;EE 
varFF 
additionalAudiencesFF 
=FF 
newFF 
[FF  
]FF  !
{FF" #
$strFF# :
,FF: ;
$strFF; S
}FFT U
;FFU V
tokenDescriptorGG 
.GG 
ClaimsGG 
=GG 
newGG 

DictionaryGG )
<GG) *
stringGG* 0
,GG0 1
objectGG2 8
>GG8 9
{HH 
{II #
JwtRegisteredClaimNamesII 
.II 
AudII  
,II  !
additionalAudiencesII" 5
}II6 7
}JJ 
;JJ 
varKK 
tokenCreationKK 
=KK 
tokenHandlerKK "
.KK" #
CreateTokenKK# .
(KK. /
tokenDescriptorKK/ >
)KK> ?
;KK? @
varLL 
tokenLL 
=LL 
tokenHandlerLL 
.LL 

WriteTokenLL %
(LL% &
tokenCreationLL& 3
)LL3 4
;LL4 5
returnMM 
tokenMM	 
;MM 
}NN 
}OO Ó
e/home/lambo-ubuntu/Authentication/Authentifications/Repositories/JwtBearerAuthenticationRepository.cs
	namespace 	
Authentifications
 
. 
Repositories (
;( )
public 
class -
!JwtBearerAuthenticationRepository .
{ 
private 
readonly	 

ApiContext 
context $
;$ %
public		 -
!JwtBearerAuthenticationRepository		 )
(		) *

ApiContext		* 4
context		5 <
)		< =
{

 
this 
. 
context 
= 
context 
; 
} 
public 
UtilisateurDto 
GetUserByFilter &
(& '
string' -
email. 3
,3 4
bool5 9
?9 :
	adminOnly; D
=E F
nullG K
)K L
{ 

IQueryable 
< 
UtilisateurDto 
> 
query "
=# $
context% ,
., -
GetUsersDataAsync- >
(> ?
)? @
.@ A
ResultA G
.G H
AsQueryableH S
(S T
)T U
;U V
if 
( 
! 
string 
. 
IsNullOrEmpty 
( 
email !
)! "
)" #
{ 
query 
=	 

query 
. 
Where 
( 
u 
=> 
u 
. 
Email #
!# $
.$ %
ToUpper% ,
(, -
)- .
==/ 1
email2 7
.7 8
ToUpper8 ?
(? @
)@ A
)A B
;B C
if 
( 
	adminOnly 
. 
HasValue 
&& 
	adminOnly &
.& '
Value' ,
&&- /
query0 5
.5 6
Any6 9
(9 :
u: ;
=>< >
u? @
.@ A
RoleA E
==F H
UtilisateurDtoI W
.W X
	PrivilegeX a
.a b
Administrateurb p
)p q
)q r
{ 
query 	
=
 
query 
. 
Where 
( 
u 
=> 
u 
. 
Role #
==$ &
UtilisateurDto' 5
.5 6
	Privilege6 ?
.? @
Administrateur@ N
)N O
;O P
} 
else 
if 

( 
	adminOnly 
. 
HasValue 
&& !
	adminOnly" +
.+ ,
Value, 1
&&2 4
query5 :
.: ;
Any; >
(> ?
u? @
=>A C
uD E
.E F
RoleF J
==K M
UtilisateurDtoN \
.\ ]
	Privilege] f
.f g
Utilisateurg r
)r s
)s t
{ 
throw 	
new
 *
AuthentificationBasicException ,
(, -
$str- h
)h i
;i j
} 
} 
return 
query	 
. 
FirstOrDefault 
( 
) 
!  
;  !
} 
} åJ
>/home/lambo-ubuntu/Authentication/Authentifications/Program.cs
var 
builder 
= 
WebApplication 
. 
CreateBuilder *
(* +
args+ /
)/ 0
;0 1
var "
MyAllowSpecificOrigins 
= 
$str 6
;6 7
builder 
. 
Services 
. 
AddControllers 
(  
)  !
;! "
builder 
. 
Services 
. 
	Configure 
< 
ApiBehaviorOptions -
>- .
(. /
options/ 6
=>7 9
{ 
options 
. 	+
SuppressModelStateInvalidFilter	 (
=) *
true+ /
;/ 0
} 
) 
; 
builder 
. 
Services 
. #
AddEndpointsApiExplorer (
(( )
)) *
;* +
builder 
. 
Services 
. 
AddSwaggerGen 
( 
opt "
=># %
{ 
opt 
. 

SwaggerDoc 
( 
$str 
, 
new 
OpenApiInfo $
{ 
Title 
= 	
$str
 *
,* +
Description 
= 
$str M
,M N
Version 	
=
 
$str 
, 
Contact 	
=
 
new 
OpenApiContact 
{ 
Name   
=   	
$str  
 
,   
Email!! 
=!!	 

$str!! #
}"" 
}## 
)## 
;## 
var%% 
xmlFilename%% 
=%% 
$"%% 
{%% 
Assembly%% 
.%%  
GetExecutingAssembly%% 3
(%%3 4
)%%4 5
.%%5 6
GetName%%6 =
(%%= >
)%%> ?
.%%? @
Name%%@ D
}%%D E
$str%%E I
"%%I J
;%%J K
opt&& 
.&& 
IncludeXmlComments&& 
(&& 
Path&& 
.&& 
Combine&& $
(&&$ %

AppContext&&% /
.&&/ 0
BaseDirectory&&0 =
,&&= >
xmlFilename&&? J
)&&J K
)&&K L
;&&L M
}(( 
)(( 
;(( 
builder)) 
.)) 
Services)) 
.)) 
AddCors)) 
()) 
options))  
=>))! #
{** 
options++ 
.++ 	
	AddPolicy++	 
(++ 
name++ 
:++ "
MyAllowSpecificOrigins++ /
,++/ 0
policy,, 
=>,, 
{-- 
policy.. 
... 
AllowAnyOrigin.. 
(.. 
).. 
.//	 

AllowAnyMethod//
 
(// 
)// 
.00	 

AllowAnyHeader00
 
(00 
)00 
;00 
}11 
)11 	
;11	 

}22 
)22 
;22 
builder44 
.44 
Configuration44 
.55 
SetBasePath55 
(55 
	Directory55 
.55 
GetCurrentDirectory55 +
(55+ ,
)55, -
)55- .
.66 
AddJsonFile66 
(66 
$"66 
$str66 
{66 
builder66 $
.66$ %
Environment66% 0
.660 1
EnvironmentName661 @
}66@ A
$str66A F
"66F G
,66G H
optional66I Q
:66Q R
false66S X
,66X Y
reloadOnChange66Z h
:66h i
false66j o
)66o p
;66p q
builder88 
.88 
Services88 
.88 
AddHttpClient88 
(88 
)88  
;88  !
builder99 
.99 
Services99 
.99 
	AddScoped99 
<99 

ApiContext99 %
>99% &
(99& '
)99' (
;99( )
builder:: 
.:: 
Services:: 
.:: #
AddControllersWithViews:: (
(::( )
)::) *
;::* +
builder;; 
.;; 
Services;; 
.;; 

AddRouting;; 
(;; 
);; 
;;; 
builder<< 
.<< 
Services<< 
.<< "
AddHttpContextAccessor<< '
(<<' (
)<<( )
;<<) *
builder== 
.== 
Services== 
.== 
AddDataProtection== "
(==" #
)==# $
;==$ %
builder>> 
.>> 
Services>> 
.>> 
AddHealthChecks>>  
(>>  !
)>>! "
;>>" #
builderFF 
.FF 
ServicesFF 
.FF 
	AddScopedFF 
<FF 
	IJwtTokenFF $
,FF$ %*
JwtBearerAuthenticationServiceFF& D
>FFD E
(FFE F
)FFF G
;FFG H
builderMM 
.MM 
ServicesMM 
.MM 
	AddScopedMM 
<MM -
!JwtBearerAuthenticationRepositoryMM <
>MM< =
(MM= >
)MM> ?
;MM? @
builderNN 
.NN 
ServicesNN 
.NN 
	AddScopedNN 
<NN *
JwtBearerAuthenticationServiceNN 9
>NN9 :
(NN: ;
)NN; <
;NN< =
builderPP 
.PP 
ServicesPP 
.PP 
AddSingletonPP 
<PP 
IConfigurationPP ,
>PP, -
(PP- .
builderPP. 5
.PP5 6
ConfigurationPP6 C
)PPC D
;PPD E
builderQQ 
.QQ 
ServicesQQ 
.QQ 

AddLoggingQQ 
(QQ 
)QQ 
;QQ 
builderRR 
.RR 
ServicesRR 
.RR 
AddAuthorizationRR !
(RR! "
)RR" #
;RR# $
builderTT 
.TT 
ServicesTT 
.TT 
AddAuthenticationTT "
(TT" #
$strTT# 8
)TT8 9
.UU 
	AddSchemeUU 
<UU '
AuthenticationSchemeOptionsUU '
,UU' ((
AuthentificationBasicServiceUU) E
>UUE F
(UUF G
$strUUG \
,UU\ ]
optionsUU^ e
=>UUf h
{UUi j
}UUk l
)UUl m
;UUm n
var}} 
app}} 
=}} 	
builder}}
 
.}} 
Build}} 
(}} 
)}} 
;}} 
appÖÖ 
.
ÖÖ 
UseMiddleware
ÖÖ 
<
ÖÖ *
ValidationHandlingMiddleware
ÖÖ .
>
ÖÖ. /
(
ÖÖ/ 0
)
ÖÖ0 1
;
ÖÖ1 2
ifÜÜ 
(
ÜÜ 
app
ÜÜ 
.
ÜÜ 
Environment
ÜÜ 
.
ÜÜ 
IsDevelopment
ÜÜ !
(
ÜÜ! "
)
ÜÜ" #
)
ÜÜ# $
{áá 
app
àà 
.
àà 

UseSwagger
àà 
(
àà 
)
àà 
;
àà 
app
ââ 
.
ââ 
UseSwaggerUI
ââ 
(
ââ 
con
ââ 
=>
ââ 
{
ää 
con
ãã 
.
ãã 
SwaggerEndpoint
ãã 
(
ãã 
$str
ãã 0
,
ãã0 1
$str
ãã2 P
)
ããP Q
;
ããQ R
con
çç 
.
çç 
RoutePrefix
çç 
=
çç 
string
çç 
.
çç 
Empty
çç !
;
çç! "
}
èè 
)
èè 
;
èè 
}êê 
appëë 
.
ëë 
UseCors
ëë 
(
ëë $
MyAllowSpecificOrigins
ëë "
)
ëë" #
;
ëë# $
appíí 
.
íí !
UseHttpsRedirection
íí 
(
íí 
)
íí 
;
íí 
appìì 
.
ìì 

UseRouting
ìì 
(
ìì 
)
ìì 
;
ìì 
appîî 
.
îî 
UseAuthentication
îî 
(
îî 
)
îî 
;
îî 
appïï 
.
ïï 
UseAuthorization
ïï 
(
ïï 
)
ïï 
;
ïï 
appññ 
.
ññ 
UseEndpoints
ññ 
(
ññ 
	endpoints
ññ 
=>
ññ 
{
óó 
	endpoints
òò 
.
òò 
MapControllers
òò 
(
òò 
)
òò 
;
òò 
	endpoints
ôô 
.
ôô 
MapHealthChecks
ôô 
(
ôô 
$str
ôô %
)
ôô% &
;
ôô& '
	endpoints
öö 
.
öö 
MapGet
öö 
(
öö 
$str
öö 
,
öö 
async
öö $
context
öö% ,
=>
öö- /
{
õõ 
await
úú 
context
úú	 
.
úú 
Response
úú 
.
úú 

WriteAsync
úú $
(
úú$ %
$str
úú% ;
)
úú; <
;
úú< =
}
ùù 
)
ùù 
;
ùù 
}
ûû 
)
ûû 
;
ûû 
appüü 
.
üü 
Run
üü 
(
üü 
)
üü 	
;
üü	 
í
L/home/lambo-ubuntu/Authentication/Authentifications/Models/UtilisateurDto.cs
	namespace 	
Authentifications
 
. 
Models "
{ 
public 

record 
UtilisateurDto  
{		 
[ 	
Key	 
] 
[ 	
DatabaseGenerated	 
( #
DatabaseGeneratedOption 2
.2 3
Identity3 ;
); <
]< =
public 
Guid 
ID 
{ 
get 
; 
set !
;! "
}# $
[ 	
Required	 
] 
public 
string 
? 
Nom 
{ 
get  
;  !
set" %
;% &
}' (
[ 	
Required	 
( 
ErrorMessage 
=  
$str! 6
)6 7
]7 8
[ 	
EmailAddress	 
( 
ErrorMessage "
=# $
$str% D
)D E
]E F
public 
string 
? 
Email 
{ 
get "
;" #
set$ '
;' (
}) *
public 
enum 
	Privilege 
{ 
Administrateur  .
,. /
Utilisateur0 ;
}< =
[ 	
EnumDataType	 
( 
typeof 
( 
	Privilege &
)& '
)' (
]( )
[ 	
Required	 
] 
public 
	Privilege 
Role 
{ 
get  #
;# $
set% (
;( )
}* +
[ 	
Required	 
] 
[ 	
Category	 
( 
$str 
) 
] 
public 
string 
? 
Pass 
{ 
get !
;! "
set# &
;& '
}( )
public   
bool   
CheckHashPassword   %
(  % &
string  & ,
password  - 5
)  5 6
{!! 	
return"" 
BCrypt"" 
."" 
Net"" 
."" 
BCrypt"" $
.""$ %
Verify""% +
(""+ ,
password"", 4
,""4 5
Pass""6 :
)"": ;
;""; <
}## 	
}$$ 
}%% Ü
I/home/lambo-ubuntu/Authentication/Authentifications/Models/TokenResult.cs
	namespace 	
Authentifications
 
. 
Models "
{ 
public 
class 
TokenResult 
{ 
public 
bool	 
Response 
{ 
get 
; 
set !
;! "
}# $
public 
string	 
? 
Message 
{ 
get 
; 
set  #
;# $
}% &
public 
string	 
? 
Token 
{ 
get 
; 
set !
;! "
}# $
} 
} ˇ	
J/home/lambo-ubuntu/Authentication/Authentifications/Models/ErrorMessage.cs
	namespace 	
Authentifications
 
. 
Models "
{ 
public 

class 
ErrorMessage 
{ 
public 
string 
? 
Type 
{ 
get !
;! "
set# &
;& '
}( )
public 
string 
? 
Title 
{ 
get "
;" #
set$ '
;' (
}) *
public 
string 
? 
Detail 
{ 
get  #
;# $
set% (
;( )
}* +
public 
int 
Status 
{ 
get 
;  
set! $
;$ %
}& '
public		 
string		 
?		 
TraceId		 
{		  
get		! $
;		$ %
set		& )
;		) *
}		+ ,
public

 
string

 
?

 
Message

 
{

  
get

! $
;

$ %
set

& )
;

) *
}

+ ,
} 
} ª<
_/home/lambo-ubuntu/Authentication/Authentifications/Middlewares/ValidationHandlingMiddleware.cs
	namespace 	
Authentifications
 
. 
Middlewares '
;' (
public 
class (
ValidationHandlingMiddleware )
{ 
private 
readonly	 
RequestDelegate !
_next" '
;' (
public (
ValidationHandlingMiddleware $
($ %
RequestDelegate% 4
next5 9
)9 :
{		 
_next

 
=

 	
next


 
;

 
} 
public 
async 
Task 
InvokeAsync 
( 
HttpContext *
context+ 2
)2 3
{ 
try 
{ 
await 
_next	 
( 
context 
) 
; 
} 
catch 
( 	
	Exception	 
ex 
) 
{ 
await  
HandleExceptionAsync	 
( 
context %
,% &
ex' )
)) *
;* +
} 
if 
( 
context 
. 
Items 
. 
ContainsKey 
(  
$str  7
)7 8
&&9 ;
!< =
context= D
.D E
ResponseE M
.M N

HasStartedN X
)X Y
{ 
var 
validationErrors 
= 
( 
List 
<  
string  &
>& '
)' (
context( /
./ 0
Items0 5
[5 6
$str6 M
]M N
!N O
;O P
context 

.
 
Response 
. 

StatusCode 
=  
$num! $
;$ %
context 

.
 
Response 
. 
ContentType 
=  !
$str" 4
;4 5
var 
response 
= 
new 
{ 
Type 
=	 

$str 2
,2 3
Title 	
=
 
$str =
,= >
Status   

=   
$num   
,   
Errors!! 

=!! 
validationErrors!! 
}"" 
;"" 
await## 
context##	 
.## 
Response## 
.## 
WriteAsJsonAsync## *
(##* +
response##+ 3
)##3 4
;##4 5
}$$ 
}%% 
private&& 
async&&	 
Task&&  
HandleExceptionAsync&& (
(&&( )
HttpContext&&) 4
context&&5 <
,&&< =
	Exception&&> G
	exception&&H Q
)&&Q R
{'' 
context(( 	
.((	 

Response((
 
.(( 
ContentType(( 
=((  
$str((! 3
;((3 4
context)) 	
.))	 

Response))
 
.)) 

StatusCode)) 
=)) 
StatusCodes))  +
.))+ ,(
Status500InternalServerError)), H
;))H I
var** 
response** 
=** 
new** 
ErrorMessage** !
{++ 
TraceId,, 

=,, 
context,, 
.,, 
TraceIdentifier,, $
,,,$ %
Message-- 

=-- 
	exception-- 
.-- 
Message-- 
,-- 
Type.. 
=.. 	
$str..
 
,.. 
Title// 
=//	 

$str// *
,//* +
Status00 	
=00
 
StatusCodes00 
.00 (
Status500InternalServerError00 4
}11 
;11 
switch22 
(22	 

	exception22
 
)22 
{33 
case44 *
AuthentificationBasicException44 &
:44& '
context55 
.55 
Response55 
.55 

StatusCode55 
=55  !
StatusCodes55" -
.55- .
Status403Forbidden55. @
;55@ A
response66 
.66 
Type66 
=66 
$str66 )
;66) *
response77 
.77 
Title77 
=77 
$str77 C
;77C D
response88 
.88 
Detail88 
=88 
$str88 K
;88K L
response99 
.99 
Status99 
=99 
StatusCodes99 !
.99! "
Status403Forbidden99" 4
;994 5
response:: 
.:: 
TraceId:: 
=:: 
context:: 
.:: 
TraceIdentifier:: .
;::. /
response;; 
.;; 
Message;; 
=;; 
	exception;;  
.;;  !
Message;;! (
;;;( )
break<< 	
;<<	 

case>> #
AuthenticationException>> 
:>>  
context?? 
.?? 
Response?? 
.?? 

StatusCode?? 
=??  !
StatusCodes??" -
.??- .!
Status401Unauthorized??. C
;??C D
response@@ 
.@@ 
Type@@ 
=@@ 
$str@@ "
;@@" #
responseAA 
.AA 
TitleAA 
=AA 
$strAA +
;AA+ ,
responseBB 
.BB 
StatusBB 
=BB 
StatusCodesBB !
.BB! "!
Status401UnauthorizedBB" 7
;BB7 8
responseCC 
.CC 
TraceIdCC 
=CC 
contextCC 
.CC 
TraceIdentifierCC .
;CC. /
responseDD 
.DD 
MessageDD 
=DD 
	exceptionDD  
.DD  !
MessageDD! (
;DD( )
breakEE 	
;EE	 

caseGG  
KeyNotFoundExceptionGG 
:GG 
contextHH 
.HH 
ResponseHH 
.HH 

StatusCodeHH 
=HH  !
StatusCodesHH" -
.HH- .
Status404NotFoundHH. ?
;HH? @
responseII 
.II 
TypeII 
=II 
$strII 
;II 
responseJJ 
.JJ 
TitleJJ 
=JJ 
$strJJ <
;JJ< =
responseKK 
.KK 
StatusKK 
=KK 
StatusCodesKK !
.KK! "
Status404NotFoundKK" 3
;KK3 4
breakLL 	
;LL	 

caseNN 
ArgumentExceptionNN 
:NN 
contextOO 
.OO 
ResponseOO 
.OO 

StatusCodeOO 
=OO  !
StatusCodesOO" -
.OO- .
Status400BadRequestOO. A
;OOA B
responsePP 
.PP 
TypePP 
=PP 
$strPP  
;PP  !
responseQQ 
.QQ 
TitleQQ 
=QQ 
$strQQ 1
;QQ1 2
responseRR 
.RR 
StatusRR 
=RR 
StatusCodesRR !
.RR! "
Status400BadRequestRR" 5
;RR5 6
breakSS 	
;SS	 

defaultTT 

:TT
 
breakUU 	
;UU	 

}VV 
awaitWW 
contextWW 
.WW 
ResponseWW 
.WW 
WriteAsJsonAsyncWW )
(WW) *
responseWW* 2
)WW2 3
;WW3 4
}XX 
}YY µ$
_/home/lambo-ubuntu/Authentication/Authentifications/Middlewares/JwtBearerAuthorizationServer.cs
	namespace		 	
Authentifications		
 
;		 
public 
class (
JwtBearerAuthorizationServer )
:* +!
AuthenticationHandler, A
<A B
JwtBearerOptionsB R
>R S
{ 
public (
JwtBearerAuthorizationServer $
($ %
IOptionsMonitor% 4
<4 5
JwtBearerOptions5 E
>E F
optionsG N
,N O
ILoggerFactory 
logger 
, 

UrlEncoder 
encoder 
, 
ISystemClock 
clock 
) 
: 
base 
( 
options 
, 
logger 
, 
encoder  
,  !
clock" '
)' (
{ 
} 
	protected 

override 
Task 
< 
AuthenticateResult +
>+ ,#
HandleAuthenticateAsync- D
(D E
)E F
{ 
if 
( 
! 
Request 
. 
Headers 
. 
ContainsKey "
(" #
$str# 2
)2 3
)3 4
return 	
Task
 
. 

FromResult 
( 
AuthenticateResult ,
., -
Fail- 1
(1 2
$str2 P
)P Q
)Q R
;R S
try 
{ 
var 

authHeader 
= %
AuthenticationHeaderValue -
.- .
Parse. 3
(3 4
Request4 ;
.; <
Headers< C
[C D
$strD S
]S T
)T U
;U V
if 
( 
! 

authHeader 
. 
Scheme 
. 
Equals  
(  !
$str! )
,) *
StringComparison+ ;
.; <
OrdinalIgnoreCase< M
)M N
)N O
{ 
return 

Task 
. 

FromResult 
( 
AuthenticateResult -
.- .
Fail. 2
(2 3
$str3 W
)W X
)X Y
;Y Z
} 
var!! 
authHeaderKey!! 
=!! 

authHeader!! !
.!!! "
	Parameter!!" +
;!!+ ,
var"" %
tokenValidationParameters""  
=""! "
Options""# *
.""* +%
TokenValidationParameters""+ D
;""D E
if$$ 
($$ %
tokenValidationParameters$$  
==$$! #
null$$$ (
)$$( )
return%% 

Task%% 
.%% 

FromResult%% 
(%% 
AuthenticateResult%% -
.%%- .
Fail%%. 2
(%%2 3
$str%%3 q
)%%q r
)%%r s
;%%s t
var'' 
tokenHandler'' 
='' 
new'' #
JwtSecurityTokenHandler'' 1
(''1 2
)''2 3
;''3 4
SecurityToken(( 
securityToken(( 
;(( 
var)) 
	principal)) 
=)) 
tokenHandler)) 
.))  
ValidateToken))  -
())- .
authHeaderKey)). ;
,)); <%
tokenValidationParameters))= V
,))V W
out))X [
securityToken))\ i
)))i j
;))j k
var,, 
ticket,, 
=,, 
new,,  
AuthenticationTicket,, (
(,,( )
	principal,,) 2
,,,2 3
Scheme,,4 :
.,,: ;
Name,,; ?
),,? @
;,,@ A
return-- 	
Task--
 
.-- 

FromResult-- 
(-- 
AuthenticateResult-- ,
.--, -
Success--- 4
(--4 5
ticket--5 ;
)--; <
)--< =
;--= >
}.. 
catch// 
(// 	
	Exception//	 
ex// 
)// 
{00 
return11 	
Task11
 
.11 

FromResult11 
(11 
AuthenticateResult11 ,
.11, -
Fail11- 1
(111 2
$"112 4
$str114 M
{11M N
ex11N P
.11P Q
Message11Q X
}11X Y
"11Y Z
)11Z [
)11[ \
;11\ ]
}22 
}33 
}44 Õ
X/home/lambo-ubuntu/Authentication/Authentifications/Middlewares/ContextPathMiddleware.cs
	namespace 	
Authentifications
 
. 
Middlewares '
{ 
public 

class !
ContextPathMiddleware &
{ 
private 
readonly 
RequestDelegate (
_next) .
;. /
private 
readonly 

PathString #
_contextPath$ 0
;0 1
public !
ContextPathMiddleware $
($ %
RequestDelegate% 4
next5 9
,9 :
string; A
contextPathB M
)M N
{ 	
_next		 
=		 
next		 
;		 
_contextPath

 
=

 
new

 

PathString

 )
(

) *
contextPath

* 5
)

5 6
;

6 7
} 	
public 
async 
Task 
InvokeAsync %
(% &
HttpContext& 1
context2 9
)9 :
{ 	
if 
( 
context 
. 
Request 
.  
Path  $
.$ %
StartsWithSegments% 7
(7 8
_contextPath8 D
,D E
outF I
varJ M
remainingPathN [
)[ \
)\ ]
{ 
context 
. 
Request 
.  
Path  $
=% &
remainingPath' 4
;4 5
await 
_next 
( 
context #
)# $
;$ %
} 
else 
{ 
context 
. 
Response  
.  !

StatusCode! +
=, -
StatusCodes. 9
.9 :
Status404NotFound: K
;K L
} 
} 	
} 
} ÿ
a/home/lambo-ubuntu/Authentication/Authentifications/Middlewares/AuthentificationBasicException.cs
	namespace 	
Authentifications
 
. 
Middlewares '
;' (
public 
class *
AuthentificationBasicException +
:, -
	Exception- 6
{ 
public *
AuthentificationBasicException &
(& '
string' -
message. 5
)5 6
:7 8
base9 =
(= >
message> E
)E F
{G H
}I J
} ›
K/home/lambo-ubuntu/Authentication/Authentifications/Interfaces/IJwtToken.cs
	namespace 	
Authentifications
 
. 

Interfaces &
{ 
public 
	interface 
	IJwtToken 
{ 
bool 
CheckUserSecret 
( 
string 

secretPass (
)( )
;) *
Task 
< 
TokenResult 
> 
GetToken 
( 
string #
email$ )
)) *
;* +
}		 
}

 ¿%
X/home/lambo-ubuntu/Authentication/Authentifications/Controllers/AccessTokenController.cs
	namespace 	
Authentifications
 
. 
Controllers '
;' (
[ 
Route 
( 
$str 
) 
] 
public		 
class		 !
AccessTokenController		 "
:		# $
ControllerBase		% 3
{

 
private 
readonly	 *
JwtBearerAuthenticationService 0
jwtToken1 9
;9 :
private 
readonly	 

ApiContext 
context $
;$ %
private 
readonly	 (
AuthentificationBasicService .
basic/ 4
;4 5
public !
AccessTokenController 
( *
JwtBearerAuthenticationService <
jwtToken= E
,E F

ApiContextG Q
contextR Y
,Y Z(
AuthentificationBasicService[ w
basicx }
)} ~
{ 
this 
. 
jwtToken 
= 
jwtToken 
; 
this 
. 
context 
= 
context 
; 
this 
. 
basic 
= 
basic 
; 
} 
[ 
HttpPost 

(
 
$str 
) 
] 
public 
async 
Task 
< 
ActionResult 
>  
Authentificate! /
(/ 0
[0 1
EmailAddress1 =
]= >
string? E
emailF K
,K L
[M N
DataTypeN V
(V W
DataTypeW _
._ `
Password` h
)h i
]i j
stringk q
passwordr z
)z {
{ 
if 
( 
! 

ModelState 
. 
IsValid 
) 
{ 
var 
validationErrors 
= 

ModelState $
.$ %
Values% +
.   

SelectMany   
(   
v   
=>   
v   
.   
Errors   
)   
.!! 
Select!! 
(!! 
e!! 
=>!! 
e!! 
.!! 
ErrorMessage!! 
)!!  
."" 
ToList"" 
("" 
)"" 
;"" 
HttpContext## 
.## 
Items## 
[## 
$str## ,
]##, -
=##. /
validationErrors##0 @
;##@ A
return$$ 	

StatusCode$$
 
($$ 
$num$$ 
)$$ 
;$$ 
}%% 
var&& 
isAuthenticated&& 
=&& 
await&& 
basic&& #
.&&# $
AuthenticateAsync&&$ 5
(&&5 6
email&&6 ;
,&&; <
password&&= E
)&&E F
;&&F G
if'' 
('' 
!'' 
isAuthenticated'' 
)'' 
{(( 
return)) 	
Unauthorized))
 
()) 
new)) 
{)) 
Errors)) #
=))$ %
$str))& A
}))B C
)))C D
;))D E
}** 
var,, 
result,, 
=,, 
await,, 
jwtToken,, 
.,, 
GetToken,, &
(,,& '
email,,' ,
),,, -
;,,- .
if-- 
(-- 
!-- 
result-- 
.-- 
Response-- 
)-- 
{.. 
return// 	
Unauthorized//
 
(// 
new// 
{// 
result// #
.//# $
Message//$ +
}//, -
)//- .
;//. /
}00 
return22 
CreatedAtAction22	 
(22 
nameof22 
(22  
Authentificate22  .
)22. /
,22/ 0
new221 4
{225 6
result227 =
.22= >
Token22> C
}22D E
)22E F
;22F G
}33 
[66 
HttpGet66 	
(66	 

$str66
 
)66 
]66 
public77 
async77 
Task77 
<77 
ActionResult77 
>77  
Users77! &
(77& '
)77' (
{88 
return99 
Ok99	 
(99 
await99 
context99 
.99 
GetUsersDataAsync99 +
(99+ ,
)99, -
)99- .
;99. /
}:: 
};; Õ,
Q/home/lambo-ubuntu/Authentication/Authentifications/DataBaseContext/ApiContext.cs
	namespace		 	
Authentifications		
 
.		 
DataBaseContext		 +
;		+ ,
public

 
class

 

ApiContext

 
{ 
private 
readonly	 

HttpClient 
_httpClient (
;( )
private 
readonly	 
IConfiguration  
configuration! .
;. /
private 
readonly	 
ILogger 
< 

ApiContext $
>$ %
_logger& -
;- .
public 

ApiContext 
( 
IConfiguration !
configuration" /
,/ 0

HttpClient1 ;
_httpClient< G
,G H
ILoggerI P
<P Q

ApiContextQ [
>[ \
logger] c
)c d
{ 
this 
. 
_httpClient 
= 
_httpClient  
;  !
this 
. 
configuration 
= 
configuration $
;$ %
_logger 	
=
 
logger 
; 
var 
BaseUrl 
= 
configuration 
[ 
$str 3
]3 4
;4 5
var 
certificateFile 
= 
configuration %
[% &
$str& 8
]8 9
;9 :
var 
certificatePassword 
= 
configuration )
[) *
$str* @
]@ A
;A B
var 
certificate 
= 
new 
X509Certificate2 (
(( )
certificateFile) 8
,8 9
certificatePassword: M
)M N
;N O
var 
handler 
= 
new 
HttpClientHandler %
(% &
)& '
;' (
handler 	
.	 

ClientCertificates
 
. 
Add  
(  !
certificate! ,
), -
;- .
handler 	
.	 
5
)ServerCertificateCustomValidationCallback
 3
=4 5
(6 7
httpRequestMessage7 I
,I J
certK O
,O P
	certChainQ Z
,Z [
sslPolicyErrors\ k
)k l
=>m o
{ 
if 
( 
sslPolicyErrors 
== 
System  
.  !
Net! $
.$ %
Security% -
.- .
SslPolicyErrors. =
.= >
None> B
)B C
{ 
return 

true 
; 
}   
_logger"" 

.""
 
LogError"" 
("" 
$str"" 7
,""7 8
sslPolicyErrors""9 H
)""H I
;""I J
return## 	
false##
 
;## 
}$$ 
;$$ 
_httpClient&& 
=&& 
new&& 

HttpClient&& 
(&& 
handler&& &
)&&& '
{'' 
BaseAddress(( 
=(( 
new(( 
Uri(( 
((( 
BaseUrl((  
)((  !
})) 
;)) 
}** 
public77 
async77 
Task77 
<77 
List77 
<77 
UtilisateurDto77 &
>77& '
>77' (
GetUsersDataAsync77) :
(77: ;
)77; <
{88 
try99 
{:: 
var;; 
request;; 
=;; 
new;; 
HttpRequestMessage;; &
(;;& '

HttpMethod;;' 1
.;;1 2
Get;;2 5
,;;5 6
$str;;7 ]
);;] ^
;;;^ _
var<< 
response<< 
=<< 
await<< 
_httpClient<< "
.<<" #
	SendAsync<<# ,
(<<, -
request<<- 4
)<<4 5
;<<5 6
response== 

.==
 #
EnsureSuccessStatusCode== "
(==" #
)==# $
;==$ %
if>> 
(>> 
response>> 
.>> 
ReasonPhrase>> 
==>> 
$str>> +
)>>+ ,
{?? 
throw@@ 
new@@	 
	Exception@@ 
(@@ 
$"@@ 
{@@ 
(@@ 
int@@ 
)@@ 
response@@ '
.@@' (

StatusCode@@( 2
}@@2 3
$str@@3 q
"@@q r
)@@r s
;@@s t
}AA 
varBB 
contentBB 
=BB 
awaitBB 
responseBB 
.BB 
ContentBB &
.BB& '
ReadAsStringAsyncBB' 8
(BB8 9
)BB9 :
;BB: ;
varCC 
utilisateursCC 
=CC 
JsonConvertCC  
.CC  !
DeserializeObjectCC! 2
<CC2 3
ListCC3 7
<CC7 8
UtilisateurDtoCC8 F
>CCF G
>CCG H
(CCH I
contentCCI P
)CCP Q
!CCQ R
;CCR S
returnDD 
utilisateursDD	 
;DD 
}FF 
catchFF 
(FF	 

	ExceptionFF
 
exFF 
)FF 
{GG 
_loggerHH 
.HH 
LogErrorHH 
(HH 
exHH 
,HH 
$strHH :
)HH: ;
;HH; <
throwII 
;II 	
}JJ 
}TT 
}UU 