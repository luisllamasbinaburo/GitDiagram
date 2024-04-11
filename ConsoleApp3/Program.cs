using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

class Program
{
    static int CANVAS_SIZE_X = 0;
    static int CANVAS_SIZE_Y = 0;

    const int CANVAS_MARGIN_LEFT = 230;
    const int CANVAS_MARGIN_RIGHT = 50;
    const int CANVAS_MARGIN_Y = 50;

    const int DIAGRAM_SIZE_X = 2000;

    const int CELL_SIZE_X = 150;
    const int CELL_SIZE_Y = 100;

    const int NODE_RADIUS = 18;
    const int NODE_BORDER = 3;
    const int NODE_OFFSET = 4;

    const int LINE_CORNER_RADIUS = 40;
    const int LINE_THICKNESS = 3;
    const int TRIANGLE_SIZE = 10;

    struct CommitCoordinates
    {
        public int Commit;
        public int Branch;

        public CommitCoordinates(int commit, int branch)
        {
            Commit = commit;
            Branch = branch;
        }
    }

    struct Link
    {
        public CommitCoordinates from;
        public CommitCoordinates to;

        public int Start_Commit;
        public int Start_Branch;
        public int End_Commit;
        public int End_Branch;

        public Link(int start_Commit, int start_Branch, int end_Commit, int end_Branch)
        {
            Start_Commit = start_Commit;
            Start_Branch = start_Branch;
            End_Commit = end_Commit;
            End_Branch = end_Branch;

            from = new CommitCoordinates(start_Commit, start_Branch);
            to = new CommitCoordinates(end_Commit, end_Branch);
        }
    }

    static string[] BranchNames = {
        "Master",
        "HotFix",
        "Release",
        "Dev",
        "Feature 1",
        "Feature 2"
    };

    static CommitCoordinates[] Commits = { };

    static Link[] Links = { };

    static void Main(string[] args)
    {
        Commits = new[] {
            new CommitCoordinates(2, 1),
            new CommitCoordinates(3, 2),
            new CommitCoordinates(4, 1),
        };

        Links = new[] {
           new Link(2, 1, 3, 2),
           new Link(2, 1, 4, 1),
           new Link(3, 2, 4, 1),
        };

        CANVAS_SIZE_X = CANVAS_MARGIN_LEFT + DIAGRAM_SIZE_X + CANVAS_MARGIN_Y;
        CANVAS_SIZE_Y = (BranchNames.Length - 1) * CELL_SIZE_Y + 2 * CANVAS_MARGIN_Y;

        Bitmap bitmap = CreateGitDiagram(CANVAS_SIZE_X, (BranchNames.Length - 1) * CELL_SIZE_Y + 2 * CANVAS_MARGIN_Y);
        bitmap.Save("git_diagram.png", ImageFormat.Png);

        Process.Start("git_diagram.png");
        Console.WriteLine("Diagrama de Git generado con éxito.");
    }

    static Bitmap CreateGitDiagram(int width, int height)
    {
        Bitmap bitmap = new Bitmap(width, height);

        using (Graphics g = Graphics.FromImage(bitmap))
        {
            g.Clear(Color.FromArgb(24, 20, 29));


            Font font = new Font("Segoe", 24);

            for (int i = 0; i < BranchNames.Length; i++)
            {
                DrawBranch(g, font, i, BranchNames[i]);
            }


            foreach (var commit in Commits)
            {
                DrawCommit(g, commit);
            }

            foreach (var link in Links)
            {
                DrawArrowBetweenCommits(g, link);
            }

            return bitmap;
        }
    }

    private static void DrawBranch(Graphics g, Font font, int i, string branchName)
    {
        Pen branchLinePen = new Pen(GetBranchColor(i), 2);

        float[] dashValues = { 4, 4 };
        branchLinePen.DashPattern = dashValues;

        int y = CANVAS_MARGIN_Y + i * CELL_SIZE_Y;
        g.DrawLine(branchLinePen, CANVAS_MARGIN_LEFT, y, CANVAS_SIZE_X - CANVAS_MARGIN_RIGHT, y);

        SizeF textSize = g.MeasureString(branchName, font);
        g.DrawString(branchName, font, Brushes.WhiteSmoke, CANVAS_MARGIN_LEFT - 20 - textSize.Width, y - textSize.Height / 2);
    }

    static void DrawCommit(Graphics g, CommitCoordinates coordinates)
    {
        int x = coordinates.Commit * CELL_SIZE_X + CANVAS_MARGIN_LEFT;
        int y = coordinates.Branch * CELL_SIZE_Y + CANVAS_MARGIN_Y;

        using (Brush brush = new SolidBrush(Color.FromArgb(24, 20, 29)))
        {
            g.FillEllipse(brush, x - NODE_RADIUS - NODE_BORDER - NODE_OFFSET, y - NODE_RADIUS - NODE_BORDER - NODE_OFFSET, (NODE_RADIUS + NODE_BORDER + NODE_OFFSET) * 2, (NODE_RADIUS + NODE_BORDER + NODE_OFFSET) * 2);
        }

        using (Brush brush = new SolidBrush(GetBranchBorderColor(coordinates.Branch)))
        {
            g.FillEllipse(brush, x - NODE_RADIUS - NODE_BORDER, y - NODE_RADIUS - NODE_BORDER, (NODE_RADIUS + NODE_BORDER) * 2, (NODE_RADIUS + NODE_BORDER) * 2);
        }

        using (Brush brush = new SolidBrush(GetBranchColor(coordinates.Branch)))
        {
            g.FillEllipse(brush, x - NODE_RADIUS, y - NODE_RADIUS, NODE_RADIUS * 2, NODE_RADIUS * 2);
        }
    }

    static void DrawArrowBetweenCommits(Graphics g, Link link)
    {
        var from = link.from;
        var to = link.to;

        int startX = from.Commit * CELL_SIZE_X + CANVAS_MARGIN_LEFT;
        int endX = to.Commit * CELL_SIZE_X + CANVAS_MARGIN_LEFT;
        int startY = from.Branch * CELL_SIZE_Y + CANVAS_MARGIN_Y;
        int endY = to.Branch * CELL_SIZE_Y + CANVAS_MARGIN_Y;

        int midX = (endX + startX) / 2;


        var color = Color.FromArgb(200, 200, 200);
        using (Pen arrowPen = new Pen(color, LINE_THICKNESS))
        {

            GraphicsPath path = new GraphicsPath();

            path.AddLine(startX + NODE_RADIUS + NODE_BORDER + NODE_OFFSET, startY, midX - LINE_CORNER_RADIUS, startY);

            if (endY > startY)
            {
                path.AddArc(midX - LINE_CORNER_RADIUS, startY, LINE_CORNER_RADIUS, LINE_CORNER_RADIUS, 270, 90);
                path.AddArc(midX, endY - LINE_CORNER_RADIUS, LINE_CORNER_RADIUS, LINE_CORNER_RADIUS, 180, -90);
            }
            else if (endY < startY)
            {
                path.AddArc(midX - LINE_CORNER_RADIUS, startY - LINE_CORNER_RADIUS, LINE_CORNER_RADIUS, LINE_CORNER_RADIUS, 90, -90);
                path.AddArc(midX, endY, LINE_CORNER_RADIUS, LINE_CORNER_RADIUS, 180, 90);
            }

            path.AddLine(midX + LINE_CORNER_RADIUS, endY, endX - NODE_RADIUS - NODE_BORDER - NODE_OFFSET - 5, endY);

            g.DrawPath(arrowPen, path);

            Point[] trianglePoints = new Point[4];
            trianglePoints[0] = new Point(endX - NODE_RADIUS - NODE_BORDER - NODE_OFFSET - TRIANGLE_SIZE - 5, endY - TRIANGLE_SIZE);
            trianglePoints[1] = new Point(endX - NODE_RADIUS - NODE_BORDER - NODE_OFFSET - TRIANGLE_SIZE, endY);
            trianglePoints[2] = new Point(endX - NODE_RADIUS - NODE_BORDER - NODE_OFFSET - TRIANGLE_SIZE - 5, endY + TRIANGLE_SIZE);
            trianglePoints[3] = new Point(endX - NODE_RADIUS - NODE_BORDER - NODE_OFFSET, endY);

            g.FillPolygon(new SolidBrush(color), trianglePoints);
        }
    }

    static Color GetBranchColor(int branch)
    {
        var rst = Color.FromArgb(227, 200, 0); // yellow

        if (branch == 0) rst = Color.FromArgb(27, 161, 226); // blue
        if (branch == 1) rst = Color.FromArgb(216, 0, 115); // pink
        if (branch == 2) rst = Color.FromArgb(96, 169, 23); // green
        if (branch == 3) rst = Color.FromArgb(240, 163, 10); // orange

        return rst;
    }

    static Color GetBranchBorderColor(int branch)
    {
        var rst = Color.FromArgb(176, 149, 0);

        if (branch == 0) rst = Color.FromArgb(0, 110, 175);
        if (branch == 1) rst = Color.FromArgb(165, 0, 64);
        if (branch == 2) rst = Color.FromArgb(45, 118, 0);
        if (branch == 3) rst = Color.FromArgb(189, 112, 0);

        return rst;
    }
}
