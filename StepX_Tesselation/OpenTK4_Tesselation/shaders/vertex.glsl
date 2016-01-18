#version 430

uniform mat4 modelview_matrix;
uniform mat3 nm;

layout (location=0) in vec3 in_position;
layout (location=1) in vec3 in_normal;

out vec3 n;

void main(void)
{
	gl_Position = modelview_matrix * vec4 ( in_position, 1.0 );
	n           = normalize ( nm * in_normal );
}
