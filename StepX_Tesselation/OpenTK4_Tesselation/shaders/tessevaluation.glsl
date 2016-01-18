#version 430 core

uniform mat4 projection_matrix;

in vec3 n[];
out vec3 normal;

patch in vec3 b_300;		// Bezier patch coefficients for vertices
patch in vec3 b_030;
patch in vec3 b_003;
patch in vec3 b_210;
patch in vec3 b_120;
patch in vec3 b_021;
patch in vec3 b_012;
patch in vec3 b_102;
patch in vec3 b_201;
patch in vec3 b_111;

patch in vec3 n_200;		// Bezier  coefficients for normals
patch in vec3 n_020;
patch in vec3 n_002;
patch in vec3 n_110;
patch in vec3 n_011;
patch in vec3 n_101;

layout(triangles, equal_spacing) in;

void main(void)
{
	float	u = gl_TessCoord.x;
	float	v = gl_TessCoord.y;
	float	w = gl_TessCoord.z;
	
	vec3	p = (b_300*u + 3.0*b_210*v + 3.0*b_201*w)*u*u + (b_030*v + 3.0*b_120*u + 3.0*b_021*w)*v*v + (b_003*w + 3.0*b_012*v + 3.0*b_102*u)*w*w + 6.0*b_111*u*v*w;
	vec3	n = n_200*u*u + n_020*v*v + n_002*w*w + 2.0*n_110*u*v + 2.0*n_011*v*w + 2.0*n_101*u*w;

	gl_Position = projection_matrix * vec4 ( p, 1.0 );
	normal      = normalize ( n );
}