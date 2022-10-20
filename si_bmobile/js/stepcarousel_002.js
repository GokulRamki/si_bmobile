stepcarousel.setup({
galleryid: 'carousel', //id of carousel DIV
beltclass: 'carouselinner', //class of inner "belt" DIV containing all the panel DIVs
panelclass: 'panel', //class of panel DIVs each holding content
autostep: {enable:true, moveby:1, pause:6000},
panelbehavior: {speed:200, wraparound:true, persist:false, wrapbehavior:'pushpull'},
defaultbuttons: {enable: false, moveby: 1, leftnav: ['arrowl.gif', -10, 100], rightnav: ['arrowr.gif', -10, 100]},
statusvars: ['statusA', 'statusB', 'statusC'], // Register 3 "status" variables
contenttype: ['inline'] // content type <--No comma following the very last parameter, always!
})
