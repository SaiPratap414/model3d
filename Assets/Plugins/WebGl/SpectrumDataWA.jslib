var Test = {
  $analyzers: {},

  StartSampling: function (namePtr, duration, bufferSize) {
    var acceptableDistance = 0.075;

    var name = UTF8ToString(namePtr);
    if(analyzers[name] != null) return;

    var analyser = null;
    var source = null;

    try {
      console.log("WEBAudio is defined: ", typeof WEBAudio !== 'undefined');
      if (typeof WEBAudio !== 'undefined') {
        console.log("WEBAudio.audioInstances: ", WEBAudio.audioInstances);
        // Convert audioInstances to an array
        var audioInstancesArray = Object.keys(WEBAudio.audioInstances).map(key => WEBAudio.audioInstances[key]);
        console.log("audioInstancesArray: ", audioInstancesArray);
        console.log("audioInstancesArray.length: ", audioInstancesArray.length);
      }
      console.log("audioInstancesArray.length > 1: ", audioInstancesArray.length > 1);
      console.log("||")
      if (typeof WEBAudio != 'undefined' && audioInstancesArray.length > 1) {
        for (var i = audioInstancesArray.length - 1; i >= 0; i--) {
          console.log("inside for, iteration: ", i);
          var pSource = audioInstancesArray[i].source;
          if (pSource != null && pSource.buffer != null && Math.abs(pSource.buffer.duration - duration) < acceptableDistance) {
            source = pSource;
            break;
          }
        }

        if (source == null) {
          console.log("No matching source found");
          return false;
        }
        analyser = source.context.createAnalyser();
        analyser.fftSize = bufferSize * 2;
        source.connect(analyser);
        console.log("this is Before Assing:" ,analyser)
        analyzers[name] = {
          analyser: analyser,
          source: source
        };

        console.log("This is Analayer: ", analyzers[name].analyser);
        console.log("This is Source: ", analyzers[name].source);
        return true;
      }
    } catch (e) {
      console.log("Failed to connect analyser to source " + e);
      if (analyser != null && source != null) {
        source.disconnect(analyser);
      }
    }

    return false;
  },

  StartSampling_OLD: function (namePtr, duration, bufferSize) {
    var acceptableDistance = 0.075;

    var name = UTF8ToString(namePtr);
    if(analyzers[name] != null) return;

    var analyser = null;
    var source = null;

    try {
      console.log(typeof WEBAudio != 'undefined')
      console.log(WEBAudio.audioInstances.length > 1)
      console.log("||")
      if (typeof WEBAudio != 'undefined' && WEBAudio.audioInstances.length > 1) {
        for (var i = WEBAudio.audioInstances.length - 1; i >= 0; i--) {
          console.log("inside for")
          var pSource = WEBAudio.audioInstances[i].source;
          if (pSource != null && pSource.buffer != null && Math.abs(pSource.buffer.duration - duration) < acceptableDistance) {
            source = pSource;
            console.log(source);
            break;
          }
        }

        if (source == null) {
          return false;
        }
        analyzer = source.context.createAnalyser();
        analyzer.fftSize = bufferSize * 2;
        source.connect(analyzer);

        analyzers[name] = {
          analyzer: analyzer,
          source: source
        };
        return true;
      }
    } catch (e) {
      console.log("Failed to connect analyser to source " + e);
      if (analyzer != null && source != null) {
        source.disconnect(analyzer);
      }
    }

    return false;
  },

  CloseSampling: function(namePtr) {
    var name = UTF8ToString(namePtr);
    var analyzerObj = analyzers[name];

    if (analyzerObj != null) {
      try {
        analyzerObj.source.disconnect(analyzerObj.analyzer);
        console.log("Deleated: ", analyzers[name])
        delete analyzers[name];
        return true;
      } catch (e) {
        console.log("Failed to disconnect analyser " + name + " from source " + e);
      }
    }

    return false;
  },

  GetSamples: function(namePtr, bufferPtr, bufferSize) {
    var name = UTF8ToString(namePtr);

    if (analyzers[name] == null) {
        console.log("Analyzer not found for name: " + name);
        return false;
    }

    try {
        var buffer = new Uint8Array(Module.HEAPU8.buffer, bufferPtr, Float32Array.BYTES_PER_ELEMENT * bufferSize);
        buffer = new Float32Array(buffer.buffer, buffer.byteOffset, bufferSize);

        var analyzerObj = analyzers[name];

        if (analyzerObj == null) {
            console.log("Could not find analyzer " + name + " to get lipsync data for");
            return false;
        }

        if (analyzerObj.analyser == null) {
            console.log("Analyzer object is null for name: " + name);
            return false;
        }

        if (typeof analyzerObj.analyser.getFloatTimeDomainData !== 'function') {
            console.log("getFloatTimeDomainData method is not defined on analyzer for name: " + name);
            return false;
        }

        analyzerObj.analyser.getFloatTimeDomainData(buffer);

        for (var i = 0; i < buffer.length; i++) {
            buffer[i] /= 4;
        }

        return true;
    } catch (e) {
        console.log("Failed to get lipsync sample data: " + e);
        console.error(e.stack);  // Add stack trace for better debugging
    }

    return false;
  },


  GetSamples_OG: function(namePtr, bufferPtr, bufferSize) {
    var name = UTF8ToString(namePtr);
    console.log("Name: ",name)
    if (analyzers[name] == null) return;
    try {
      var buffer = new Uint8Array(Module.HEAPU8.buffer, bufferPtr, Float32Array.BYTES_PER_ELEMENT * bufferSize);
      buffer = new Float32Array(buffer.buffer, buffer.byteOffset, bufferSize);

      var analyzerObj = analyzers[name];
      console.log("AnalyerObj: ", analyzerObj)
      if (analyzerObj == null) {
        console.log("Could not find analyzer " + name + " to get lipsync data for");
        return false;
      }

      analyzerObj.analyzer.getFloatTimeDomainData(buffer);
      for (var i = 0; i < buffer.length; i++) {
        buffer[i] /= 4;
      }
      return true;
    } catch (e) {
      console.log("Failed to get lipsync sample data " + e);
    }

    return false;
  }

};

autoAddDeps(Test, '$analyzers');
mergeInto(LibraryManager.library, Test);