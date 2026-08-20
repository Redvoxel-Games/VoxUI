namespace VoxUI.Rendering;

public static class Shaders
{
    public static string RectVertex =
        """
        #version 330 core
        
        layout (location = 0) in vec3 vertPos;
        
        uniform vec2 rectPos;
        uniform vec2 rectSize;
        uniform vec2 screenSize;
        
        uniform vec2 UVMin;
        uniform vec2 UVMax;
        
        out vec2 localPos;
        out vec2 textureCoord;
        
        void main()
        {
            localPos = vertPos.xy;
        
            vec2 pos = rectPos + vertPos.xy * rectSize;
            vec2 normalizedPos = pos / screenSize;
            vec2 ndcPos = normalizedPos * 2.0 - 1.0;
        
            ndcPos.y = -ndcPos.y;
        
            gl_Position = vec4(ndcPos, vertPos.z, 1.0);
            
            textureCoord = mix(
                UVMin,
                UVMax,
                vertPos.xy
            );
        }
        """;

    public static string RectFragment =
        """
        #version 330 core
        
        uniform vec2 rectSize;
        uniform float rectLineWidth;
        uniform vec4 cornerRadii;
        uniform vec4 rectColor;
        
        in vec2 localPos;
        
        out vec4 FragColor;
        
        float roundedBoxSDF(vec2 p, vec2 b, vec4 r)
        {
            // Select the radius based on the quadrant.
            float radius;
        
            if (p.x < 0.0)
                radius = (p.y < 0.0) ? r.x : r.z;
            else
                radius = (p.y < 0.0) ? r.y : r.w;
        
            vec2 q = abs(p) - b + radius;
        
            return min(max(q.x, q.y), 0.0)
                 + length(max(q, 0.0))
                 - radius;
        }
        
        void main()
        {
            // Convert from 0..1 to pixel coordinates centered on the rectangle.
            vec2 p = localPos * rectSize - rectSize * 0.5;
        
            vec2 halfSize = rectSize * 0.5;
        
            // Corner radii in pixels.
            vec4 radii = cornerRadii;
        
            // Prevent radii from extending beyond the rectangle.
            float maxRadius = min(halfSize.x, halfSize.y);
        
            radii = min(radii, maxRadius);
        
            // Signed distance to the rounded rectangle.
            float d = roundedBoxSDF(
                p,
                halfSize,
                radii
            );
        
            // Filled rectangle.
            if (rectLineWidth < 0.0)
            {
                if (d > 0.0)
                    discard;
        
                FragColor = rectColor;
                return;
            }
        
            // Border.
            float innerD = roundedBoxSDF(
                p,
                halfSize - rectLineWidth,
                max(radii - rectLineWidth, 0.0)
            );
        
            // Outside outer boundary or inside inner boundary.
            if (d > 0.0 || innerD < 0.0)
                discard;
        
            FragColor = rectColor;
        }
        """;

    public static string TextFragment =
        """
        #version 330 core
        
        in vec2 textureCoord;
        
        uniform sampler2D fontTexture;
        uniform vec4 textColor;
        
        out vec4 FragColor;
        
        void main()
        {
            float alpha = texture(fontTexture, textureCoord).r;
        
            FragColor = vec4(textColor.rgb, textColor.a * alpha);
        }
        """;

    public static string ImageFragment =
        """
        #version 330 core
        
        in vec2 textureCoord;
        
        uniform sampler2D image;
        
        out vec4 FragColor;
        
        void main()
        {
            vec4 col = texture(image, textureCoord);
        
            FragColor = col;
        }
        """;
}