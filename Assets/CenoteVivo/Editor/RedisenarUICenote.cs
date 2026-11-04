using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine.U2D;
using UnityEditor.U2D;

public static class RedisenarUICenote
{
    const string Root="Assets/CenoteVivo/UI/", Sprites=Root+"Sprites/";
    static CenoteUITheme theme;
    static RectTransform R(Transform t)=>(RectTransform)t;
    static Transform Find(Transform p,string name){var t=p.Find(name);if(!t)throw new Exception("Falta "+name+" en "+p.name);return t;}
    static Transform Add(Transform p,string name)
    {
        var t=p.Find(name);if(t)return t;
        var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(p,false);return go.transform;
    }
    static void Box(Transform t,float x0,float y0,float x1,float y1)
    {
        var r=R(t);r.anchorMin=new Vector2(x0,y0);r.anchorMax=new Vector2(x1,y1);r.pivot=new Vector2(.5f,.5f);r.offsetMin=r.offsetMax=Vector2.zero;
    }
    static void TopLeft(Transform t,float x,float y,float w,float h)
    {
        var r=R(t);r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);
    }
    static UnityEngine.UI.Image Img(Transform t,Color color,Sprite sprite=null,bool sliced=false)
    {
        var i=t.GetComponent<UnityEngine.UI.Image>();if(!i)i=t.gameObject.AddComponent<UnityEngine.UI.Image>();
        i.color=color;i.sprite=sprite;i.type=sliced?UnityEngine.UI.Image.Type.Sliced:UnityEngine.UI.Image.Type.Simple;i.raycastTarget=false;return i;
    }
    static TMPro.TextMeshProUGUI Text(Transform t,string value,float size,Color color,bool bold=false,TMPro.TextAlignmentOptions align=TMPro.TextAlignmentOptions.MidlineLeft)
    {
        var x=t.GetComponent<TMPro.TextMeshProUGUI>();if(!x)x=t.gameObject.AddComponent<TMPro.TextMeshProUGUI>();
        x.font=theme.font;x.text=value;x.fontSize=size;x.color=color;x.fontStyle=bold?TMPro.FontStyles.Bold:TMPro.FontStyles.Normal;
        x.alignment=align;x.raycastTarget=false;x.enableAutoSizing=false;x.overflowMode=TMPro.TextOverflowModes.Overflow;return x;
    }
    static void Height(Transform t,float h)
    {
        var e=t.GetComponent<LayoutElement>();if(!e)e=t.gameObject.AddComponent<LayoutElement>();e.minHeight=h;e.preferredHeight=h;e.flexibleHeight=0;
    }
    static Sprite Sprite(string name)=>AssetDatabase.LoadAssetAtPath<Sprite>(Sprites+name+".png");
    static void Fade(Transform t)
    {
        if(!t.GetComponent<CanvasGroup>())t.gameObject.AddComponent<CanvasGroup>();
        if(!t.GetComponent<CenoteUIScreenFade>())t.gameObject.AddComponent<CenoteUIScreenFade>();
    }
    static void Outline(Transform parent)
    {
        var t=Add(parent,"BordeSutil");Box(t,0,0,1,1);t.SetAsFirstSibling();
        Img(t,new Color(.68f,.85f,.84f,.34f),theme.outline,true);
    }
    static UnityEngine.UI.Image Icon(Transform parent,string id,float x0,float y0,float x1,float y1)
    {
        var t=Add(parent,"Icono");Box(t,x0,y0,x1,y1);var i=Img(t,theme.text,Sprite(id));i.preserveAspect=true;return i;
    }
    static CenoteUIButton Style(UnityEngine.UI.Button button,string icon,string title,bool primary=false,CenoteUIButton.Accent accent=CenoteUIButton.Accent.Primary,bool arrows=false)
    {
        var t=button.transform;var label=Find(t,"Etiqueta");
        var txt=Text(label,title,30,theme.text,true);Box(label,icon==null?.08f:.22f,.12f,arrows?.82f:.96f,.88f);
        txt.enableAutoSizing=true;txt.fontSizeMin=24;txt.fontSizeMax=30;txt.overflowMode=TMPro.TextOverflowModes.Truncate;
        var surface=Img(t,theme.glass,theme.rounded,true);surface.raycastTarget=true;button.targetGraphic=surface;
        var colors=button.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(.91f,1, .98f,1);colors.pressedColor=Color.white;colors.selectedColor=Color.white;colors.disabledColor=new Color(.6f,.65f,.65f,.6f);colors.fadeDuration=.12f;button.colors=colors;
        Outline(t);
        var style=t.GetComponent<CenoteUIButton>();if(!style)style=t.gameObject.AddComponent<CenoteUIButton>();
        style.theme=theme;style.label=txt;style.primaryAction=primary;style.accent=accent;
        if(icon!=null)style.icon=Icon(t,icon,.05f,.24f,.18f,.76f);
        if(arrows){var arrow=Add(t,"Flecha");Box(arrow,.86f,.29f,.95f,.71f);style.secondaryIcon=Img(arrow,theme.text,Sprite("IconoFlecha"));style.secondaryIcon.preserveAspect=true;}
        var old=t.Find("Indicador");if(old)old.gameObject.SetActive(false);
        style.Apply();return style;
    }
    static void MakeTexture(string path,bool border,bool gradient=false)
    {
        if(System.IO.File.Exists(path))return;
        int w=gradient?256:64,h=gradient?1:64;var tx=new Texture2D(w,h,TextureFormat.RGBA32,false);
        for(int y=0;y<h;y++)for(int x=0;x<w;x++)
        {
            float alpha;
            if(gradient){float t=x/(float)(w-1);alpha=t<.52f?Mathf.Lerp(.88f,.55f,t/.52f):Mathf.Lerp(.55f,.85f,(t-.52f)/.48f);}
            else
            {
                float dx=Mathf.Max(17.5f-x,x-45.5f,0),dy=Mathf.Max(17.5f-y,y-45.5f,0);
                float d=18-Mathf.Sqrt(dx*dx+dy*dy);alpha=Mathf.Clamp01(d);
                if(border)alpha*=1-Mathf.Clamp01(d-2);
            }
            tx.SetPixel(x,y,gradient?new Color(.01f,.07f,.09f,alpha):new Color(1,1,1,alpha));
        }
        tx.Apply();System.IO.File.WriteAllBytes(path,tx.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tx);
    }
    static void PrepareAssets()
    {
        MakeTexture(Sprites+"SuperficieRedonda.png",false);MakeTexture(Sprites+"BordeRedondo.png",true);MakeTexture(Sprites+"VeloGradiente.png",false,true);
        AssetDatabase.Refresh();
        foreach(var path in System.IO.Directory.GetFiles(Sprites,"*.png"))
        {
            var importer=AssetImporter.GetAtPath(path) as TextureImporter;if(!importer)continue;
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.mipmapEnabled=false;
            importer.alphaIsTransparency=true;importer.filterMode=FilterMode.Bilinear;
            if(path.EndsWith("Redonda.png")||path.EndsWith("Redondo.png"))importer.spriteBorder=new Vector4(22,22,22,22);
            importer.maxTextureSize=path.EndsWith("CenoteReal.png")?1024:256;
            importer.textureCompression=path.EndsWith("CenoteReal.png")?TextureImporterCompression.Compressed:TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
        theme=AssetDatabase.LoadAssetAtPath<CenoteUITheme>(Root+"TemaCenote.asset");
        if(!theme){theme=ScriptableObject.CreateInstance<CenoteUITheme>();AssetDatabase.CreateAsset(theme,Root+"TemaCenote.asset");}
        theme.rounded=Sprite("SuperficieRedonda");theme.outline=Sprite("BordeRedondo");theme.font=AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if(!theme.rounded||!theme.font||!Sprite("CenoteReal"))throw new Exception("Falta una fuente o sprite de UI.");
        EditorUtility.SetDirty(theme);
        var atlas=AssetDatabase.LoadAssetAtPath<SpriteAtlas>(Root+"IconosCenote.spriteatlas");
        if(!atlas)
        {
            atlas=new SpriteAtlas();var pack=atlas.GetPackingSettings();pack.enableRotation=false;pack.padding=2;atlas.SetPackingSettings(pack);
            var tex=atlas.GetTextureSettings();tex.generateMipMaps=false;tex.readable=false;atlas.SetTextureSettings(tex);
            var entries=new List<UnityEngine.Object>();foreach(var p in System.IO.Directory.GetFiles(Sprites,"Icono*.png")){var a=AssetDatabase.LoadAssetAtPath<Texture2D>(p);if(a)entries.Add(a);}
            entries.Add(AssetDatabase.LoadAssetAtPath<Texture2D>(Sprites+"SuperficieRedonda.png"));entries.Add(AssetDatabase.LoadAssetAtPath<Texture2D>(Sprites+"BordeRedondo.png"));
            atlas.Add(entries.ToArray());atlas.SetIncludeInBuild(true);AssetDatabase.CreateAsset(atlas,Root+"IconosCenote.spriteatlas");
        }
    }
    static void Back(Transform t,Transform page)
    {
        t.SetParent(page,false);TopLeft(t,65,28,295,100);
        var b=t.GetComponent<UnityEngine.UI.Button>();Style(b,"IconoVolver","Volver al inicio");
        var style=t.GetComponent<CenoteUIButton>();style.label.fontSize=27;style.label.fontSizeMax=27;style.label.fontSizeMin=22;
    }
    static void LayoutMenu(MenuCenote m)
    {
        var bg=m.fondo.transform;var image=Img(bg,Color.white,Sprite("CenoteReal"));image.preserveAspect=false;
        var aspect=bg.GetComponent<AspectRatioFitter>();if(!aspect)aspect=bg.gameObject.AddComponent<AspectRatioFitter>();aspect.aspectMode=AspectRatioFitter.AspectMode.EnvelopeParent;aspect.aspectRatio=16f/9f;
        var old=bg.Find("ProfundidadAgua");if(old)old.gameObject.SetActive(false);
        var veil=Add(bg,"VeloOscuro");Box(veil,0,0,1,1);Img(veil,Color.white,Sprite("VeloGradiente"));veil.SetAsFirstSibling();
        var line=bg.Find("LineaSuperior");if(line){Img(line,theme.primary);R(line).sizeDelta=new Vector2(0,3);line.SetAsLastSibling();}
        var home=m.inicio.transform;Fade(home);
        var view=Find(home,"Viewport");Box(view,.055f,.065f,.945f,.935f);
        var content=Find(view,"Contenido");Box(content,0,0,1,1);
        var id=Find(content,"Identidad");Box(id,0,0,.54f,1);
        var nav=Find(content,"Navegacion");Box(nav,.59f,.06f,1,.94f);Img(nav,theme.glass,theme.rounded,true);Outline(nav);
        var shade=Add(content,"SombraTarjeta");Box(shade,.592f,.048f,1,.932f);Img(shade,new Color(0,0,0,.22f),theme.rounded,true);shade.SetSiblingIndex(nav.GetSiblingIndex());
        var kicker=Find(id,"Lema");Box(kicker,0,.91f,1,.97f);Text(kicker,"YUCATÁN  /  PATRIMONIO NATURAL",25,theme.primary,true);
        var title=Find(id,"Titulo");Box(title,0,.61f,1,.87f);var tit=Text(title,"Cenote <color=#35D6B1>Vivo</color>",110,theme.text,true);tit.enableAutoSizing=true;tit.fontSizeMin=75;tit.fontSizeMax=110;
        var sub=Find(id,"Subtitulo");Box(sub,0,.43f,.95f,.57f);Text(sub,"Un universo subterráneo\nque merece ser cuidado.",38,theme.secondaryText);
        var logo=Find(id,"TarjetaLogoUniversidad");Box(logo,0,.12f,.72f,.32f);Img(logo,Color.white,theme.rounded,true);
        var logoimg=Find(logo,"LogoUniversidad");Box(logoimg,.05f,.10f,.95f,.90f);logoimg.GetComponent<UnityEngine.UI.Image>().preserveAspect=true;
        var university=Find(id,"Universidad");Box(university,0,.045f,.9f,.105f);Text(university,"UNIVERSIDAD MARISTA DE MÉRIDA",19,theme.secondaryText);
        var h=Find(nav,"Invitacion");Box(h,.09f,.82f,.93f,.94f);Text(h,"Tu viaje comienza aquí",37,theme.text,true);
        var subh=Find(nav,"Ayuda");Box(subh,.09f,.75f,.93f,.82f);Text(subh,"Explora. Comprende. Cuida.",26,theme.secondaryText);
        var a=Find(nav,"AccederAlCenote");Box(a,.07f,.50f,.93f,.70f);Style(a.GetComponent<UnityEngine.UI.Button>(),"IconoAgua","Acceder al cenote",true,arrows:true);
        var obj=Find(nav,"ObjetivoDelProyecto");Box(obj,.07f,.30f,.93f,.46f);Style(obj.GetComponent<UnityEngine.UI.Button>(),"IconoDocumento","Objetivo del proyecto",arrows:true);
        var cred=Find(nav,"Creditos");Box(cred,.07f,.10f,.93f,.26f);Style(cred.GetComponent<UnityEngine.UI.Button>(),"IconoPersonas","Créditos",arrows:true);
        var note=Find(content,"NotaConservacion");note.SetParent(nav,false);Box(note,.15f,.015f,.94f,.09f);Text(note,"CONOCER PARA CONSERVAR",22,theme.primary,true,TMPro.TextAlignmentOptions.Center);
        var leaf=Add(nav,"HojaConservacion");Box(leaf,.075f,.026f,.14f,.075f);Img(leaf,theme.primary,Sprite("IconoHoja"));
        var back=m.botonVolver.transform;TopLeft(back,30,28,225,100);Style(back.GetComponent<UnityEngine.UI.Button>(),"IconoVolver","Inicio");
        var bs=back.GetComponent<CenoteUIButton>();bs.label.fontSize=28;bs.label.fontSizeMax=28;bs.label.fontSizeMin=24;
    }
    static void LayoutObjective(MenuCenote m)
    {
        var page=m.objetivo.transform;Fade(page);var scroll=page.GetComponent<ScrollRect>();scroll.horizontal=false;scroll.vertical=true;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=60;
        var view=Find(page,"Viewport");Box(view,.055f,.08f,.55f,.82f);
        var content=Find(view,"Contenido");var layout=content.GetComponent<VerticalLayoutGroup>();layout.enabled=true;layout.spacing=20;layout.padding=new RectOffset(0,35,10,30);layout.childControlWidth=true;layout.childControlHeight=true;layout.childForceExpandWidth=true;layout.childForceExpandHeight=false;
        var fitter=content.GetComponent<ContentSizeFitter>();fitter.enabled=true;fitter.verticalFit=ContentSizeFitter.FitMode.PreferredSize;
        Back(Find(content,"VolverInicio"),page);
        var title=Find(content,"Titulo");Height(title,76);Text(title,"Objetivo del proyecto",48,theme.text,true);
        var intro=Find(content,"Introduccion");var introText=Text(intro,"Conocer los cenotes es el primer paso para cuidarlos.",36,theme.primary,true,TMPro.TextAlignmentOptions.TopLeft);introText.textWrappingMode=TMPro.TextWrappingModes.Normal;
        var body=Find(content,"Objetivo");var bodyText=body.GetComponent<TMPro.TextMeshProUGUI>();Text(body,bodyText.text,31,theme.text,false,TMPro.TextAlignmentOptions.TopLeft).lineSpacing=7;
        var note=Find(content,"Nota");var nt=note.GetComponent<TMPro.TextMeshProUGUI>();Text(note,nt.text,25,theme.secondaryText,false,TMPro.TextAlignmentOptions.TopLeft);
        var picture=Add(page,"ImagenDelCenote");Box(picture,.59f,.32f,.945f,.82f);Img(picture,Color.white,theme.rounded,true);
        var mask=picture.GetComponent<Mask>();if(!mask)mask=picture.gameObject.AddComponent<Mask>();mask.showMaskGraphic=false;
        var img=Add(picture,"FotografiaReal");Box(img,0,0,1,1);Img(img,Color.white,Sprite("CenoteReal"));
        var shade=Add(picture,"Velo");Box(shade,0,0,1,1);Img(shade,new Color(.03f,.12f,.14f,.22f));
        var phrase=Add(page,"FraseConservacion");Box(phrase,.59f,.17f,.945f,.28f);var q=Text(phrase,"Explorar para valorar.\nConocer para conservar.",29,theme.secondaryText);q.fontStyle=TMPro.FontStyles.Italic;
        var mark=Add(page,"LineaConservacion");Box(mark,.59f,.145f,.88f,.148f);Img(mark,theme.primary);
    }
    static void LayoutCredits(MenuCenote m)
    {
        var page=m.creditos.transform;Fade(page);var scroll=page.GetComponent<ScrollRect>();scroll.horizontal=false;scroll.vertical=true;scroll.movementType=ScrollRect.MovementType.Clamped;
        var view=Find(page,"Viewport");Box(view,.08f,.055f,.92f,.76f);
        var content=Find(view,"Contenido");var layout=content.GetComponent<VerticalLayoutGroup>();layout.enabled=true;layout.spacing=4;layout.padding=new RectOffset(45,45,20,25);layout.childControlWidth=true;layout.childControlHeight=true;layout.childForceExpandWidth=true;layout.childForceExpandHeight=false;
        var fitter=content.GetComponent<ContentSizeFitter>();fitter.enabled=true;fitter.verticalFit=ContentSizeFitter.FitMode.PreferredSize;
        Back(Find(content,"VolverInicio"),page);
        var title=Find(content,"Titulo");title.SetParent(page,false);Box(title,.08f,.78f,.70f,.88f);Text(title,"Créditos",52,theme.text,true);
        var line=Add(page,"AcentoTitulo");Box(line,.08f,.77f,.20f,.774f);Img(line,theme.primary);
        var panel=Add(view,"TarjetaCreditos");Box(panel,0,0,1,1);Img(panel,theme.glass,theme.rounded,true);Outline(panel);panel.SetAsFirstSibling();
        var logo=Find(content,"TarjetaLogoUniversidad");Height(logo,145);Img(logo,Color.clear,theme.rounded,true);
        var white=Add(logo,"FondoInstitucional");Box(white,.30f,.10f,.70f,.90f);Img(white,Color.white,theme.rounded,true);white.SetAsFirstSibling();
        var li=Find(logo,"LogoUniversidad");Box(li,.32f,.16f,.68f,.84f);li.GetComponent<UnityEngine.UI.Image>().preserveAspect=true;
        string[] names={"Autor","Escuela","Semestre","Asignatura","Profesor"};
        string[] titles={"AUTOR","ESCUELA","SEMESTRE","ASIGNATURA","PROFESOR"};
        string[] values={"Jorge Carlos Trujeque Díaz","Universidad Marista de Mérida","Semestre 5","Gráficos por computadora","Luis de Jesús López Hernández"};
        string[] icons={"IconoPersonas","IconoEscuela","IconoCalendario","IconoDocumento","IconoProfesor"};
        for(int i=0;i<names.Length;i++)
        {
            var row=Find(content,names[i]);Height(row,70);var txt=Text(row,"<color=#35D6B1><size=25>"+titles[i]+"</size></color>     "+values[i],29,theme.text);
            txt.margin=new Vector4(76,0,0,0);txt.enableAutoSizing=true;txt.fontSizeMin=23;txt.fontSizeMax=29;txt.overflowMode=TMPro.TextOverflowModes.Truncate;
            var icon=Add(row,"IconoFila");var r=R(icon);r.anchorMin=r.anchorMax=new Vector2(0,.5f);r.pivot=new Vector2(0,.5f);r.anchoredPosition=new Vector2(5,0);r.sizeDelta=new Vector2(40,40);Img(icon,theme.secondaryText,Sprite(icons[i]));
        }
        foreach(Transform ch in content)if(ch.name=="Separador"){Height(ch,2);Img(ch,new Color(.7f,.85f,.85f,.16f));}
    }
    static void LayoutHUD(MenuCenote m,ControlesCenote c)
    {
        var safe=c.zonaSegura;var panel=Find(safe,"PanelEstados");var p=R(panel);p.anchorMin=new Vector2(.07f,0);p.anchorMax=new Vector2(.93f,0);p.pivot=new Vector2(.5f,0);p.anchoredPosition=new Vector2(0,22);p.sizeDelta=new Vector2(0,240);
        Img(panel,theme.glass,theme.rounded,true);Outline(panel);
        var state=Find(panel,"EstadoActual");Box(state,.02f,.74f,.98f,.98f);Text(state,"NORMAL · Agua clara y vida activa",29,theme.text,true,TMPro.TextAlignmentOptions.Center);
        var row=Find(panel,"Botones");Box(row,.025f,.28f,.975f,.72f);var h=row.GetComponent<HorizontalLayoutGroup>();h.spacing=14;h.childControlWidth=h.childControlHeight=true;h.childForceExpandWidth=h.childForceExpandHeight=true;
        Style(c.normal,"IconoHoja","Normal");Style(c.danado,"IconoDano","Dañado",accent:CenoteUIButton.Accent.Damage);Style(c.contaminado,"IconoContaminacion","Contaminado",accent:CenoteUIButton.Accent.Pollution);
        foreach(var b in new[]{c.normal,c.danado,c.contaminado}){var st=b.GetComponent<CenoteUIButton>();st.label.alignment=TMPro.TextAlignmentOptions.Center;st.label.fontSize=29;st.label.fontSizeMax=29;st.label.fontSizeMin=23;}
        var note=Find(panel,"NotaEducativa");Box(note,.03f,.025f,.97f,.24f);Text(note,"Representación visual educativa; no predice daños reales.",24,theme.secondaryText,false,TMPro.TextAlignmentOptions.Center);
        var ciclo=c.GetComponent<CicloDiaNocheCenote>();var day=ciclo.boton;TopLeft(day.transform,30,140,318,100);var dayStyle=Style(day,"IconoLuna","Cambiar a noche");dayStyle.label.fontSize=27;dayStyle.label.fontSizeMax=27;dayStyle.label.fontSizeMin=22;dayStyle.icon.color=theme.damage;
        ciclo.etiqueta=dayStyle.label;
        var hud=c.GetComponent<CenoteHUDController>();if(!hud)hud=c.gameObject.AddComponent<CenoteHUDController>();hud.ciclo=ciclo;hud.iconoDiaNoche=dayStyle.icon;hud.sol=Sprite("IconoSol");hud.luna=Sprite("IconoLuna");
        var cameras=c.GetComponent<VistasProfilerCenote>();var rowcam=Add(safe,"SelectorCamaras");Box(rowcam,.49f,0,.99f,1);
        var rr=R(rowcam);rr.anchorMin=new Vector2(.49f,1);rr.anchorMax=new Vector2(.99f,1);rr.pivot=new Vector2(.5f,1);rr.anchoredPosition=new Vector2(0,-28);rr.sizeDelta=new Vector2(0,100);
        var ch=rowcam.GetComponent<HorizontalLayoutGroup>();if(!ch)ch=rowcam.gameObject.AddComponent<HorizontalLayoutGroup>();ch.spacing=5;ch.childControlHeight=ch.childControlWidth=true;ch.childForceExpandHeight=ch.childForceExpandWidth=true;
        var names=new[]{"Vista actual","Alejada","Lejana"};var icons=new[]{"IconoOjo","IconoExpandir","IconoPaisaje"};var buttons=new UnityEngine.UI.Button[3];
        for(int i=0;i<3;i++)
        {
            var t=Add(rowcam,names[i]);var img=Img(t,theme.glass,theme.rounded,true);img.raycastTarget=true;
            var b=t.GetComponent<UnityEngine.UI.Button>();if(!b)b=t.gameObject.AddComponent<UnityEngine.UI.Button>();b.targetGraphic=img;
            var label=Add(t,"Etiqueta");Text(label,names[i],27,theme.text,true,TMPro.TextAlignmentOptions.Center);
            var st=Style(b,icons[i],names[i]);st.label.alignment=TMPro.TextAlignmentOptions.Center;st.label.fontSize=26;st.label.fontSizeMin=21;st.label.fontSizeMax=26;st.SetSelected(i==0);buttons[i]=b;
        }
        cameras.botones=buttons;Fade(c.transform);
    }
    static void MakePrefabs(MenuCenote m)
    {
        var path=Root+"Prefabs";if(!AssetDatabase.IsValidFolder(path))AssetDatabase.CreateFolder(Root.TrimEnd('/'),"Prefabs");
        var source=m.inicio.transform.Find("Viewport/Contenido/Navegacion/ObjetivoDelProyecto");
        var clone=UnityEngine.Object.Instantiate(source.gameObject);clone.name="BotonSecundario";
        var button=clone.GetComponent<UnityEngine.UI.Button>();while(button.onClick.GetPersistentEventCount()>0)UnityEventTools.RemovePersistentListener(button.onClick,0);
        PrefabUtility.SaveAsPrefabAsset(clone,path+"/BotonSecundario.prefab");UnityEngine.Object.DestroyImmediate(clone);
        var card=new GameObject("Tarjeta",typeof(RectTransform),typeof(CanvasRenderer),typeof(UnityEngine.UI.Image));var r=(RectTransform)card.transform;r.sizeDelta=new Vector2(500,300);
        var image=card.GetComponent<UnityEngine.UI.Image>();image.sprite=theme.rounded;image.type=UnityEngine.UI.Image.Type.Sliced;image.color=theme.glass;image.raycastTarget=false;
        PrefabUtility.SaveAsPrefabAsset(card,path+"/Tarjeta.prefab");UnityEngine.Object.DestroyImmediate(card);
    }
    public static string AjustarLectura()
    {
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        var m=UnityEngine.Object.FindAnyObjectByType<MenuCenote>(FindObjectsInactive.Include);
        theme=AssetDatabase.LoadAssetAtPath<CenoteUITheme>(Root+"TemaCenote.asset");
        var page=m.objetivo.transform;var view=Find(page,"Viewport");Box(view,.055f,.105f,.55f,.82f);
        var panel=Add(page,"FondoLectura");Box(panel,.04f,.035f,.565f,.845f);Img(panel,new Color(.025f,.11f,.13f,.82f),theme.rounded,true);panel.SetAsFirstSibling();
        var caption=Add(page,"IndicacionDesplazamiento");Box(caption,.055f,.045f,.53f,.095f);Text(caption,"Desliza para continuar",22,theme.secondaryText);
        foreach(var text in m.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true))if(text.overflowMode==TMPro.TextOverflowModes.Ellipsis){text.overflowMode=TMPro.TextOverflowModes.Truncate;EditorUtility.SetDirty(text);}
        var c=UnityEngine.Object.FindAnyObjectByType<ControlesCenote>(FindObjectsInactive.Include);
        foreach(var text in c.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true))if(text.overflowMode==TMPro.TextOverflowModes.Ellipsis){text.overflowMode=TMPro.TextOverflowModes.Truncate;EditorUtility.SetDirty(text);}
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);return "Contraste, aviso de desplazamiento y textos corregidos";
    }
    [MenuItem("Cenote Vivo/Interfaz/Aplicar rediseño inmersivo")]
    public static string Aplicar()
    {
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if(EditorApplication.isPlaying||scene.path!="Assets/Scenes/CenoteVivo_Peces.unity"||scene.isDirty)throw new Exception("Abrir y guardar CenoteVivo_Peces fuera de Play antes de aplicar.");
        var m=UnityEngine.Object.FindAnyObjectByType<MenuCenote>(FindObjectsInactive.Include);var c=UnityEngine.Object.FindAnyObjectByType<ControlesCenote>(FindObjectsInactive.Include);
        if(!m||!c||!m.logoUniversidad||!c.cardumen)throw new Exception("Faltan referencias del menú, logo o controles.");
        if(m.inicio.transform.Find("Viewport/Contenido/SombraTarjeta"))return "El rediseño ya está aplicado";
        EditorSceneManager.SaveScene(scene,"Assets/Scenes/CenoteVivo_AntesUIInmersiva.unity",true);
        PrepareAssets();LayoutMenu(m);LayoutObjective(m);LayoutCredits(m);LayoutHUD(m,c);MakePrefabs(m);
        foreach(var canvas in new[]{m.GetComponent<CanvasScaler>(),c.GetComponent<CanvasScaler>()}){canvas.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;canvas.referenceResolution=new Vector2(1920,1080);canvas.matchWidthOrHeight=.5f;}
        m.MostrarInicio();Canvas.ForceUpdateCanvases();EditorUtility.SetDirty(m);EditorUtility.SetDirty(c);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        Selection.activeGameObject=m.gameObject;return "Interfaz inmersiva aplicada a Inicio, Objetivo, Créditos y HUD; escena guardada";
    }
}
