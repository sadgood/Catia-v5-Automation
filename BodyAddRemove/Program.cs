﻿//2016.11.01. (thues day) 노다 사오리
//body Add remove

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MECMOD;
using PARTITF;
using System.Runtime.InteropServices;

namespace BodyAddRemove
{
    class Program
    {
        static void Main(string[] args)
        {
            INFITF.Application catia;
            try
            {
                catia = (INFITF.Application)Marshal.GetActiveObject("CATIA.Application");
            }
            catch (Exception)
            {
                catia = (INFITF.Application)Activator.CreateInstance(Type.GetTypeFromProgID("CATIA.Application"));
            }
            catia.Visible = true;

            PartDocument prtDoc = (PartDocument)catia.Documents.Add("Part");
            Part prt = prtDoc.Part;
            Bodies bdys = prt.Bodies;
            Body PartBody = bdys.Item(1);   
                        
            INFITF.Reference xypln = (INFITF.Reference)prt.OriginElements.PlaneXY;  //다른 body에서 사용가능
            ShapeFactory shpfac = (ShapeFactory)prt.ShapeFactory;

            //1 body1생성-----------------------
            Body bdy1 = bdys.Add();
            Sketches skts = bdy1.Sketches;
            Sketch skt = skts.Add(xypln);
            Factory2D fac2d = skt.OpenEdition();
          
                CreateRectangle(fac2d,prt,skt,50,50,100,100);  //사각형을 만든 method를 작성했다

            skt.CloseEdition();

            Pad p1 = shpfac.AddNewPad(skt,80);

            //2--------------------------------
            Body bdy2 = bdys.Add();
            Sketch skt2 = bdy2.Sketches.Add(xypln);
            fac2d = skt2.OpenEdition();

                Circle2D c = fac2d.CreateClosedCircle(75,75,20);

            skt2.CloseEdition();
            Pad p2 = shpfac.AddNewPad(skt2,100);

            //3----------------------------------
            Body bdy3 = bdys.Add();
            Sketch skt3 = bdy3.Sketches.Add(xypln);
            fac2d = skt3.OpenEdition();

                Point2D pt1 = fac2d.CreatePoint(75, 80);
                Point2D pt2 = fac2d.CreatePoint(70, 70);
                Point2D pt3 = fac2d.CreatePoint(80, 70);

                CreateTriangle(fac2d, prt, skt3, pt1, pt2, pt3);    //삼각형을 만든 method를 작성했다

            skt3.CloseEdition();
            Pad p3 = shpfac.AddNewPad(skt3,100);

            //----------------------------------------------
            prt.InWorkObject = PartBody;        //workobject 지정한다

            shpfac.AddNewAdd(bdy1);
            shpfac.AddNewAdd(bdy2);
            shpfac.AddNewRemove(bdy3);

            prt.Update();
        }

        //------------------------------------------------------------------------------
        //삼각형을 만든 method
        private static void CreateTriangle(Factory2D fac, Part prt, Sketch skt, Point2D p1, Point2D p2, Point2D p3)
        {
            Line2D lin1 = CreateLine(fac, p1, p2);
            Line2D lin2 = CreateLine(fac, p2, p3);
            Line2D lin3 = CreateLine(fac, p3, p1);

            CatConstraintType cntL = CatConstraintType.catCstTypeLength;
            CatConstraintType cntD = CatConstraintType.catCstTypeDistance;
           // CreateCnst(prt, skt, cntL, lin1);  //다중구속이 되어 error
            CreateCnst(prt, skt, cntL, lin2);
            CreateCnst(prt, skt, cntL, lin3);
            //CreateCnst(prt, skt, CatConstraintType.catCstTypeHorizontality, lin2);    //안되는 경우가 발생한다.
            CreateCnst(prt, skt, cntD, p1, skt.AbsoluteAxis.HorizontalReference);
            CreateCnst(prt, skt, cntD, p1, skt.AbsoluteAxis.VerticalReference);
            CreateCnst(prt, skt, cntD, p2, skt.AbsoluteAxis.HorizontalReference);
            CreateCnst(prt, skt, cntD, p2, skt.AbsoluteAxis.VerticalReference);
        }

        //사각형을 만든 method-------------------------------------------------
        private static void CreateRectangle(Factory2D fac, Part prt, Sketch skt, double x1, double y1, double x2, double y2)
        {
            Point2D p1 = fac.CreatePoint(x1, y1);
            Point2D p2 = fac.CreatePoint(x2, y1);
            Point2D p3 = fac.CreatePoint(x2, y2);
            Point2D p4 = fac.CreatePoint(x1, y2);

            Line2D lin1 = CreateLine(fac, p1, p2);
            Line2D lin2 = CreateLine(fac, p2, p3);
            Line2D lin3 = CreateLine(fac, p3, p4);
            Line2D lin4 = CreateLine(fac, p4, p1);

            CatConstraintType cntype = CatConstraintType.catCstTypeDistance;
            Constraint cnst1 = CreateCnst(prt, skt, cntype, lin1, lin3);
            Constraint cnst2 = CreateCnst(prt, skt, cntype, lin2, lin4);
            Constraint cnst3 = CreateCnst(prt, skt, cntype, lin1, skt.AbsoluteAxis.HorizontalReference);
            Constraint cnst4 = CreateCnst(prt, skt, cntype, lin2, skt.AbsoluteAxis.VerticalReference);
        }

        //구속조건을 주는 method--------------------------------------------
        private static Constraint CreateCnst(Part prt, Sketch skt, CatConstraintType cntype, INFITF.AnyObject ob1, INFITF.AnyObject ob2)
        {
            INFITF.Reference r1 = prt.CreateReferenceFromGeometry(ob1);
            INFITF.Reference r2 = prt.CreateReferenceFromGeometry(ob2);
            Constraint cnt = skt.Constraints.AddBiEltCst(cntype,r1,r2); //2객체 사이의 구속조건을 준다.
            return cnt;
        }
        // CreateCnst overloading-------------
        private static Constraint CreateCnst(Part prt, Sketch skt, CatConstraintType cntype, INFITF.AnyObject ob)
        {
            INFITF.Reference r = prt.CreateReferenceFromGeometry(ob);
             Constraint cnt = skt.Constraints.AddMonoEltCst(cntype,r);  //1객체를 구속시킨다.
             return cnt;    
        }
        //line을 만드는 method--------------------------------------------
        private static Line2D CreateLine(Factory2D fac, Point2D p1, Point2D p2)
        {
            //point에서 좌표를 추출한다
            object[] ob1 = new object[2];
            p1.GetCoordinates(ob1);

            object[] ob2 = new object[2];
            p2.GetCoordinates(ob2);

            Line2D line = fac.CreateLine((double)ob1[0], (double)ob1[1], (double)ob2[0], (double)ob2[1]);
            line.StartPoint= p1;
            line.EndPoint = p2;

            return line;
        }
    }
}
