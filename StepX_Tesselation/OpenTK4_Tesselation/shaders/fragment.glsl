#version 430

uniform mat4 modelview_matrix;

in vec3 normal;

const vec3 ambient = vec3(0.1, 0.1, 0.1);
const vec3 lightVecNormalized = normalize(vec3(0.0,1.0,2.0));
const vec3 lightColor = vec3(0.9, 0.5, 0.5);

out vec4 Color;

void main(void)
{
	
	float diffuse = clamp(dot(lightVecNormalized, normalize(normal)), 0.0, 1.0);
	Color = vec4(ambient + diffuse * lightColor, 1.0);
	//Color = vec4(0.9, 0.5, 0.5, 1.0);
}